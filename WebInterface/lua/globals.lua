function Object(t)
    local o = js.new(js.global.Object)
    for k, v in pairs(t) do
        assert(type(k) == "string" or js.typeof(k) == "symbol", "JavaScript only has string and symbol keys")
        o[k] = v
    end
    return o
end