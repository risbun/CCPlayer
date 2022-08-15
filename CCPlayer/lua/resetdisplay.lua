for monitorName in string.gmatch(settings.get("monitors"), "([^,]+)") do
    local monitor = peripheral.wrap(monitorName);
    monitor.setTextScale(1)
	
	monitor.setPaletteColor(1, colors.unpackRGB(tonumber("F0F0F0", 16)))
	monitor.setPaletteColor(2, colors.unpackRGB(tonumber("F2B233", 16)))
	monitor.setPaletteColor(4, colors.unpackRGB(tonumber("E57FD8", 16)))
	monitor.setPaletteColor(8, colors.unpackRGB(tonumber("99B2F2", 16)))
	monitor.setPaletteColor(16, colors.unpackRGB(tonumber("DEDE6C", 16)))
	monitor.setPaletteColor(32, colors.unpackRGB(tonumber("7FCC19", 16)))
	monitor.setPaletteColor(64, colors.unpackRGB(tonumber("F2B2CC", 16)))
	monitor.setPaletteColor(128, colors.unpackRGB(tonumber("4C4C4C", 16)))
	monitor.setPaletteColor(256, colors.unpackRGB(tonumber("999999", 16)))
	monitor.setPaletteColor(512, colors.unpackRGB(tonumber("4C99B2", 16)))
	monitor.setPaletteColor(1024, colors.unpackRGB(tonumber("B266E5", 16)))
	monitor.setPaletteColor(2048, colors.unpackRGB(tonumber("3366CC", 16)))
	monitor.setPaletteColor(4096, colors.unpackRGB(tonumber("7F664C", 16)))
	monitor.setPaletteColor(8192, colors.unpackRGB(tonumber("57A64E", 16)))
	monitor.setPaletteColor(16384, colors.unpackRGB(tonumber("CC4C4C", 16)))
	monitor.setPaletteColor(32768, colors.unpackRGB(tonumber("111111", 16)))
	
    monitor.setBackgroundColor(32768)
    monitor.clear()
end
