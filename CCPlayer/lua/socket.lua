local LibDeflate = require 'LibDeflate'

--local test = peripheral.wrap("left")

--local monitorIds = {9, 10, 11, 8, 5, 4, 6, 2, 3}

local monitors = {}

for monitorName in string.gmatch(settings.get("monitors"), "([^,]+)") do
	local monitor = peripheral.wrap(monitorName);

	table.insert(monitors, monitor)
	
    monitor.setTextScale(1)
    monitor.setBackgroundColor(colors.gray)
    monitor.clear()
end

local addr = "ws://home.reez.it:51413"
local ws, err = http.websocket(addr)
if not ws then
  return printError(err)
end

ws.send(arg[1])

local connectmsg = ws.receive();

info = {}
infoindex = 0
for value in string.gmatch(connectmsg, '([^,]+)') do
	info[infoindex] = value
	infoindex = infoindex + 1
end

inputwidth = tonumber(info[0])
monitorheight = 82
monitorwidth = 41

hexlength = 6

while true do

	local _, url, raw, isBinary = os.pullEvent("websocket_message")
	local msg = LibDeflate:DecompressZlib(raw)
	
	for y = 0, 2, 1 do
		for x = 0, 2, 1 do

			id = 1 + x + y * 3

			displaybytes = msg:sub(96);
			
			for row = 1, monitorheight, 1 do
				text = displaybytes:sub(1 + row * inputwidth + monitorheight * x + (monitorheight * 3 * monitorwidth * y),
							       row * inputwidth + inputwidth + monitorheight * x + (monitorheight * 3 * monitorwidth * y))

				monitors[id].setCursorPos(1, row)
				monitors[id].blit((" "):rep(#text),text,text)
			end
			
			for i=0,15,1 do
				local test = msg:sub(i*hexlength+1, i * hexlength + hexlength);
				monitors[id].setPaletteColor(2^i, colors.unpackRGB(tonumber(test,16)))
			end
		end
	end
end
