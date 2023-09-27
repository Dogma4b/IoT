packet = require("packet")
hook = require("hook")
config = require("config")

function switch (i)
    local def = {}
    return setmetatable({ i }, {
        __call = function (t, cases)
            local item = #t == 0 or t[1]
            return (cases[item] or cases[def])(item)
        end
    })
end

-----------------WIFI init-----------------

wifi.start()
wifi.mode(wifi.STATION)
wifi.sta.config(config.get("wifi.conf"))
wifi.sta.sethostname(config.get("name"))
if config.get("wifi.ipconf") then wifi.sta.setip(config.get("wifi.ipconf")) end
wifi.sta.connect()

wifi.sta.on("got_ip", function(ev, info)
    print("IP received: " .. info.ip)
    hook.Call("WIFIConnected", info.ip, info.netmask, info.gw)
end)

-----------------TCP client init-----------------

function ConnectionInit(ip, port)
    conn = net.createConnection(net.TCP, 0)

    PB = {len = 0, id = 0, frame = 0, data = nil}
    conn:on("receive", function(socket, data)
        if (PB.data == nil) then
            PB.data = data
            PB.frame = PB.frame + 1
        else
            PB.data = PB.data .. data
            PB.frame = PB.frame + 1
            if (PB.frame == 2) then
                PB.id, PB.len = struct.unpack("<I4I4", PB.data)
            end
        end
        if (#PB.data >= PB.len) and (PB.len ~= 0) then
            hook.Call("NetReceive", PB.id, PB.data:sub(9))
            PB = {len = 0, id = 0, frame = 0, data = nil}
        end
    end)

    conn:on("connection", function(socket, data)
        conn:send(packet(10,"s",node.chipid()))
        node.output(function(message) conn:send(packet(110,"s",message)) end, 1)
    end)

    conn:on("reconnection", function(socket, data)
        --print("try reconnect")
        --node.output(nil)
        tmr.create():alarm(10000, tmr.ALARM_SINGLE, function() ConnectionInit(config.get("server.ip"), config.get("server.port")) end)
    end)

    conn:connect(port,ip)
end

hook.Add("WIFIConnected", "Remote link init", function(ip) ConnectionInit(config.get("server.ip"), config.get("server.port")) end)

----------------------------------

--hook.Add("NetReceive","test",function(id,data) print(packet("si", data)) end)
hook.Add("NetReceive","Remote device console",function(id,data)
    switch(id) {
        [110] = function()
            node.input(packet("s", data))
        end,
        [111] = function()
            switch(packet("h",data)) {
                [1] = function()
                    conn:send(packet(111,"hs",1,sjson.encode(file.list())))
                end,
                [2] = function()
                    local fileName, faid = packet("sh",data:sub(3))
                    local fileNameLen = packet("h",data:sub(3))
                    if (faid == 1) then
                        fileUploadList = fileUploadList or {}
                        fileUploadList[fileName] = file.open(fileName, "w+")
                        fileUploadList[fileName]:write(encoder.fromBase64(packet("s",data:sub(7 + fileNameLen))))
                    elseif (faid == 2) then
                        fileUploadList[fileName]:write(encoder.fromBase64(packet("s",data:sub(7 + fileNameLen))))
                    elseif (faid == 3) then
                        local text, fileLen, action = packet("shh",data:sub(7 + fileNameLen))
                        fileUploadList[fileName]:write(encoder.fromBase64(text))
                        fileUploadList[fileName]:flush()
                        fileUploadList[fileName]:close()
                        fileUploadList[fileName] = nil

                        if (file.list()[fileName] == fileLen) then
                            if (action == 1) or (action == 2) then
                                node.compile(fileName)
                            end
                            if (action == 2) then
                                file.remove(fileName)
                            end
                            conn:send(packet(111,"hs",2,fileName))
                        end
                    end
                end,
                [3] = function()
                    file.rename(packet("ss",data:sub(3)))
                end,
                [4] = function()
                    local action, fileName = packet("hs",data:sub(3))
                    if (action == 1) or (action == 2) then
                        node.compile(fileName)
                    end
                    if (action == 2) then
                        file.remove(fileName)
                    end
                end,
                [5] = function()
                    file.remove(packet("s",data:sub(3)))
                end
            }
        end
    }
end)