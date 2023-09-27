-- gpio init--
gpio.mode(1,gpio.INPUT,gpio.PULLUP)
-- end --

function getWaterData()
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