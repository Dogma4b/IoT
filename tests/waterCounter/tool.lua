local response = {}
response.query = 'tool'

if(tool.cmd == "terminal") then
    node.input(tool.input)
    node.output(function(result)
        response = {
            query = 'tool',
            data = {
                cmd = 'terminal',
                client = tool.cid,
                mode = 'chip',
                result = result
            }
        }
    ws:send(sjson.encode(response))
    end)
elseif(tool.cmd == 'ls') then
    response.data = file.list()
elseif(tool.cmd == '') then

end

--ws:send(sjson.encode(response))