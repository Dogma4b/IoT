
packet = require("packet")

if file.exists('config.json') then
    Config = sjson.decode(file.open('config.json','r'));
    wifi.sta.config({ssid = Config.wifi.ssid,pwd})

else
    
end

function WifiConnect()
    wifi.start()
    wifi.mode(wifi.STATION)
    wifi.sta.config({ssid="",pwd="",auto=false})
    wifi.sta.sethostname("waterCounter")
    wifi.sta.connect()
end

WifiConnect()

--udpSocket = net.createUDPSocket()


conn = net.createConnection(net.TCP, 0)

buffer = nil
conn:on("receive", function(socket, data)
  if buffer == nil then
buffer = pck
else
buffer = buffer .. pck
end
end)

--print(struct.unpack("<I4",buffer))
-- Wait for connection before sending.
conn:on("connection", function(socket, c)
  print("Connected")
end)

function Run(msg) EnvRun(Environments["IoT"],[[pck = socket:Packet(111) pck:WriteString("]].. msg ..[[") server:Broadcast(pck)]]) end

print(select(1,struct.unpack("<hc0i4",buffer:sub(9))))
send(packet(1,"si","Hi!",1234))