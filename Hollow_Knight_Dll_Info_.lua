function onPaint()
    local infoAddress = memory.readu64(0x400000 + 0x1B1CF60)
    infoAddress = memory.readu64(infoAddress + 0x0)
    infoAddress = memory.readu64(infoAddress + 0x10)
    infoAddress = memory.readu64(infoAddress + 0x50)
    infoAddress = memory.readu64(infoAddress + 0xD08)

    if infoAddress == 0 then
        return
    end

    local textArray = {}

    local infoText = readString(infoAddress)
    for line in infoText:gmatch("[^\r\n]+") do
        table.insert(textArray, line)
    end

    guiText(textArray)
end

function guiText(textArray)
    local width, _ = gui.resolution()
    for i, v in ipairs(textArray) do
        gui.text(width, 23 * (i - 1), v)
    end
end

function readString(address)
    local text = ""
    local len = memory.readu16(address + 0x10)
    for i = 1, len do
        text = text .. string.char(memory.readu16(address + 0x12 + i * 2))
    end
    return text;
end