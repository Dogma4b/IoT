local config = {
    data = {}
}

config.file = file.open("config.json", "r")
if config.file then
    config.data = sjson.decode(config.file:read())
    config.file:close()
    config.file = nil
end

config.set = function(key, value)

    local function setVal(data, keys, value)
        local key = keys[1]
        table.remove(keys, 1)

        if(#keys > 0) then
            if(data[key] == nil) then data[key] = {} end
            setVal(data[key], keys, value)
        else
            data[key] = value
        end
    end

    setVal(config.data, config.split(key, "."), value)
    config.save()
end

config.get = function(key)
    local value = config.data
    for k,v in pairs(config.split(key, ".")) do value = value[v] end
    return value
end

config.save = function()
    file.remove("config.json")
    config.file = file.open("config.json", "a+")
    if config.file then
        config.file:write(sjson.encode(config.data))
        config.file:close()
        config.file = nil
    end
end

config.split = function(str, sep)
    local t = {}
    for key in str:gmatch("([^" .. sep .. "]+)") do table.insert(t, key) end
    return t
end

return config