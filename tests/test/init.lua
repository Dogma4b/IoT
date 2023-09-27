--gpio.mode(1,gpio.INPUT,gpio.PULLUP)

wifi.sta.disconnect()
wifi.setmode(wifi.STATION)
wifi.sta.config({ssid="",pwd=""})
wifi.sta.connect()

tmr.alarm(1,5000,1,function()
    if(wifi.sta.getip()~=nil) then
        tmr.stop(1)
        print("Connected")
        print("IP:",wifi.sta.getip())
    else
        print("Connecting")
    end
end)

function waterData()
    local water = {cold=nil,hot=nil}

    water.cold = gpio.read(5)
    water.hot = adc.read(0)

    if(water.hot <= 300) then
        water.hot = 0
    else
        water.hot = 1
    end

    return sjson.encode(water)
end