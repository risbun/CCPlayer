local LibDeflate = require 'LibDeflate'

local test = peripheral.wrap("left")

local monitorIds = {9, 10, 11, 8, 5, 4, 6, 2, 3}
local monitors = {}


for i,id in ipairs(monitorIds) do
    monitors[id] = peripheral.wrap("monitor_"..id)
    monitors[id].setTextScale(1)
    monitors[id].setBackgroundColor(colors.gray)
    monitors[id].clear()
end

local addr = "ws://localhost:6969"
local ws, err = http.websocket(addr)
if not ws then
  return printError(err)
end

ws.send(arg[1])

local connectmsg = ws.receive();

print(monitors[9].getSize())


info = {}
infoindex = 0
for value in string.gmatch(connectmsg, '([^,]+)') do
	info[infoindex] = value
	infoindex = infoindex + 1
end

inputwidth = tonumber(info[0])
monitorheight = 82
monitorwidth = 41

while true do

	local _, url, raw, isBinary = os.pullEvent("websocket_message")
	local msg = LibDeflate:DecompressZlib(raw)

	local colores = ws.receive()

	for y = 0, 2, 1 do
		for x = 0, 2, 1 do

			id = monitorIds[1 + x + y * 3]
			colorindex = 0
			for value in string.gmatch(colores, '([^,]+)') do
				monitors[id].setPaletteColor(2^colorindex, colors.unpackRGB(tonumber(value,16)))
				colorindex = colorindex + 1
			end

			for row = 1, monitorheight, 1 do
				text = msg:sub(1 + row * inputwidth + monitorheight * x + (monitorheight * 3 * monitorwidth * y),
							       row * inputwidth + inputwidth + monitorheight * x + (monitorheight * 3 * monitorwidth * y))

				monitors[id].setCursorPos(1, row)
				monitors[id].blit((" "):rep(#text),text,text)
			end
		end
	end
end
