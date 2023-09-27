
if file.exists('config.json') then
    Config = sjson.decode(file.open('config.json','r'));
    wifi.sta.config({ssid = Config.wifi.ssid,pwd})

else
    
end

function WifiConnect()
    wifi.start()
    wifi.mode(wifi.STATION)
    wifi.sta.config({ssid="",pwd="",auto=false})
    wifi.sta.connect()
end

WifiConnect()

data = struct.pack("<hil",10,20,30)
udpSocket = net.createUDPSocket()
udpSocket:send(8000,"192.168.200.1",data)