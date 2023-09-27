-- gpio init--
gpio.mode(1,gpio.INPUT,gpio.PULLUP)
-- end --

if file.open('config.json') then
    config = sjson.decode(file.read())
    file.close()
end

wifi.sta.disconnect()
wifi.setmode(wifi.STATION)
wifi.sta.config(config.wifi)
wifi.sta.sethostname(config.hostname)
wifi.sta.connect()

tmr.alarm(1,1000,1,function()
    if(wifi.sta.getip()~=nil) then
        tmr.stop(1)
        sockInit()
    end
end)

function sockInit()
    ws = websocket.createClient()
    ws:connect('ws://'..config.IoTServer.ip..':'..config.IoTServer.port)

    ws:on('connection',function(ws)
        clientAuth(ws)
    end)
    
    ws:on('receive',function(_,data,opcode)
        local tbl = sjson.decode(data)

        if(tbl.query == 'auth') then
            if(tbl.data.status == 'complete') then
                auth = true
            end
        end
        if(auth) then
            if(tbl.query == "tool") then
                tool = tbl.data
                dofile("tool.lua")
            end
        else
            clientAuth(ws)
        end
    end)
    
    ws:on('close',function(_,status)
        
    end)
end

function clientAuth(ws)
    local tbl = {
        query = 'auth',
        data = {
            id = node.chipid(),
            type = config.type,
            location = config.location,
            apiKey = config.IoTServer.apiKey
        }
    }
    ws:send(sjson.encode(tbl))
end