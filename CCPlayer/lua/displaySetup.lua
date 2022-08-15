local allMonitors = { peripheral.find("monitor") }
local monitors = {}

function GetSaveString()
	local str = ""
	for index, monitor in pairs(monitors) do
		append = ""
		if index < 9 then
			append = ","
		end
	
		str = str .. peripheral.getName(monitor) .. append
	end
	return str;
end

term.setTextColor(1)
print("checking for old setup...")
if settings.get("monitors") ~= nil then
	term.setTextColor(16384)
	print("found old config, override? y/n")
	
	for _, monitor in pairs(allMonitors) do
		monitor.setBackgroundColor(16384)
		monitor.clear()
		
		monitor.setTextColour(1)
		
		local sizeX, sizeY = monitor.getSize()
		monitor.setCursorPos(sizeX/2-7, sizeY/2)
		monitor.write("CHECK TERMINAL")
	end
	if read() ~= "y" then
		for _, monitor in pairs(allMonitors) do
			monitor.setBackgroundColor(32768)
			monitor.clear()
		end
	
		print("user aborted")
		error()
	end
end

function main()
	monitors = {}

	term.setTextColor(32)
	print("monitors ready")
	
	for _, monitor in pairs(allMonitors) do
		monitor.setBackgroundColor(32)
		monitor.clear()
		
		monitor.setTextColour(32768)
		
		local sizeX, sizeY = monitor.getSize()
		monitor.setCursorPos(sizeX/2-2, sizeY/2)
		monitor.write("READY")
	end
	
	for i=1,9,1 do
		event, monitorName, x, y = os.pullEvent("monitor_touch")
		
		monitor = peripheral.wrap(monitorName)
		
		monitor.setBackgroundColor(32768)
		monitor.clear()
		
		monitor.setTextColour(1)
		
		local sizeX, sizeY = monitor.getSize()
		monitor.setCursorPos(sizeX/2-4, sizeY/2)
		monitor.write("Display " .. i)
		
		table.insert(monitors, monitor)
	end

	for _, monitor in pairs(allMonitors) do
		monitor.setBackgroundColor(32768)
		monitor.clear()
		
		monitor.setTextColour(1)
		
		local sizeX, sizeY = monitor.getSize()
		monitor.setCursorPos(sizeX/2-8, sizeY/2)
		monitor.write("not loved sadge")
	end

	for i, monitor in pairs(monitors) do
		monitor.setBackgroundColor(16)
		monitor.clear()
		
		monitor.setTextColour(32768)
		
		local sizeX, sizeY = monitor.getSize()
		monitor.setCursorPos(sizeX/2-4, sizeY/2)
		monitor.write("Display " .. i)
	end
	
	term.setTextColor(1)
	print("save? y/n")
	if read() ~= "y" then
		for _, monitor in pairs(monitors) do
			monitor.setBackgroundColor(16384)
			monitor.clear()
		end
	
		return false
	end
	
	term.setTextColor(32)
	print("saved monitor layout: [" .. GetSaveString() .. "]")
	settings.set("monitors", GetSaveString())
	settings.save()

	for _, monitor in pairs(allMonitors) do
		monitor.setBackgroundColor(32768)
		monitor.clear()
	end
	
	return true
end

while true do
	local success = main()
	
	if success then
		break
	end
end