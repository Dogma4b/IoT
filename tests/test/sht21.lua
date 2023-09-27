local moduleName = ... 
 local M = {}
 _G[moduleName] = M

local SHTAddr = 0x40
local SDA = 5
local SCL = 6
local Ide = 0
local eTempHoldCmd        = 0xE3
local eRHumidityHoldCmd   = 0xE5
local eTempNoHoldCmd      = 0xF3
local eRHumidityNoHoldCmd = 0xF5

function M.Init(Ide,SDA,SCL)
  return i2c.setup(Ide, SDA, SCL, i2c.SLOW)
end

function M.Read_User_Reg(Ide)
  i2c.start(Ide)
  i2c.address(Ide, SHTAddr ,i2c.TRANSMITTER)
  i2c.write(Ide,0xE7)
  i2c.stop(Ide)

  i2c.start(Ide)
  i2c.address(Ide, SHTAddr,i2c.RECEIVER)
  --tmr.delay(1000)
  c=i2c.read(Ide,1)
  i2c.stop(Ide)
  return string.byte(c)
end

function M.Read_Temp(Ide)
  i2c.start(Ide)
  i2c.address(Ide, SHTAddr ,i2c.TRANSMITTER)
  i2c.write(Ide,eTempNoHoldCmd)
  i2c.stop(Ide)
  tmr.delay(100000)

  i2c.start(Ide)
  i2c.address(Ide, SHTAddr,i2c.RECEIVER)
  c=i2c.read(Ide,3)
  i2c.stop(Ide)
  print("Raw",string.byte(c,1,2))
  d=bit.clear(bit.lshift(string.byte(c,1),8)+string.byte(c,2),1,3)
  return (((d*17572)/65536)-4685)/100;
end

function M.Read_Hum(Ide)
  i2c.start(Ide)
  i2c.address(Ide, SHTAddr ,i2c.TRANSMITTER)
  i2c.write(Ide,eRHumidityNoHoldCmd)
  i2c.stop(Ide)
  tmr.delay(50000)

  i2c.start(Ide)
  i2c.address(Ide, SHTAddr,i2c.RECEIVER)
  c=i2c.read(Ide,3)
  i2c.stop(Ide)
  print("Raw",string.byte(c,1,2))
  d=bit.clear(bit.lshift(string.byte(c,1),8)+string.byte(c,2),1,3)
  return (((d*12500)/65536)-600)/100
end

--print("Speed ",Init(id,pinSDA, pinSCL))
--print("User reg",Read_User_Reg(id))

--print(string.format("%02.2f",Read_Temp(id)),"deg C")
--print(string.format("%02.2f",Read_Hum(id)),"RH")

return M