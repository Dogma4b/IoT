local packet = setmetatable({}, {__call = function(self, ...) if #{...} > 2 then  return self:pack(...) else return self:unpack(...) end end})

packet.pack = function(self, id, template, ...)
    self.lenght = 0
    self.pattern = "<I4I4" -- Little endian
    local argstemp = {}
    local args, temp = {...}, {}
    for char in template:gmatch("[^%s]") do table.insert(temp, char) end
    for key, char in pairs(temp) do
        if char == "i" then
            self:CharAdd("i4")
            self:LenghtAdd(4)
            table.insert( argstemp, args[key] )
        elseif char == "I" then
            self:CharAdd("I4")
            self:LenghtAdd(4)
            table.insert( argstemp, args[key] )
        elseif char == "h" then
            self:CharAdd("h")
            self:LenghtAdd(2)
            table.insert( argstemp, args[key] )
        elseif char == "H" then
            self:CharAdd("H")
            self:LenghtAdd(2)
            table.insert( argstemp, args[key] )
        elseif char == "l" then
            self:CharAdd("l")
            self:LenghtAdd(8)
            table.insert( argstemp, args[key] )
        elseif char == "L" then
            self:CharAdd("L")
            self:LenghtAdd(8)
            table.insert( argstemp, args[key] )
        elseif char == "f" then
            self:CharAdd("f")
            self:LenghtAdd(4)
            table.insert( argstemp, args[key] )
        elseif char == "d" then
            self:CharAdd("d")
            self:LenghtAdd(8)
            table.insert( argstemp, args[key] )
        elseif char == "s" then
            local strlen = args[key]:len()
            self:CharAdd("h")
            self:CharAdd("c")
            self:CharAdd(strlen)
            self:LenghtAdd(strlen + 2)
            table.insert( argstemp, strlen )
            table.insert( argstemp, args[key] )
        end
    end
    args = argstemp
    table.insert(args,1,id)
    table.insert(args,2,self.lenght)
    return struct.pack(self.pattern, unpack(args))
end

packet.unpack = function(self, template, pck)
    self.pattern = "<" -- Little endian
    local data, temp = {}, {}
    for char in template:gmatch("[^%s]") do table.insert(temp, char) end
    for key, char in pairs(temp) do
        if char == "i" then
            self:CharAdd("i4")
        elseif char == "I" then
            self:CharAdd("I4")
        elseif char == "h" then
            self:CharAdd("h")
        elseif char == "H" then
            self:CharAdd("H")
        elseif char == "l" then
            self:CharAdd("l")
        elseif char == "L" then
            self:CharAdd("L")
        elseif char == "f" then
            self:CharAdd("f")
        elseif char == "d" then
            self:CharAdd("d")
        elseif char == "s" then
            self:CharAdd("h")
            self:CharAdd("c0")
        end
    end

    local values = {struct.unpack(self.pattern, pck)}
    table.remove(values)

    return unpack(values)
end

packet.CharAdd = function(self, char)
    self.pattern = self.pattern .. char
end

packet.LenghtAdd = function(self, num)
    self.lenght = self.lenght + num
end

return packet