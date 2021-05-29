local gameVersion

function onStart()
    gameVersion = nil
end

function onPaint()
    local infoAddress = getInfoAddress()

    if infoAddress == 0 then
        return
    end

    local infoText = readString(infoAddress)
    local gameInfo = {}
    for line in infoText:gmatch("[^\r\n]+") do
        if line:find("^Enemy=") ~= nil then
            local hpData = splitString(line:sub(7), "|")
            for i = 1, #hpData, 3 do
                gui.text(hpData[i], hpData[i + 1], hpData[i + 2])
            end
        elseif line:find("^LineHitbox=") ~= nil then
            local hitboxData = splitString(line:sub(12), "|")
            for i = 1, #hitboxData, 5 do
                gui.line(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 3], hitboxData[i + 4])
            end
        elseif line:find("^CircleHitbox=") ~= nil then
            local hitboxData = splitString(line:sub(14), "|")
            for i = 1, #hitboxData, 4 do
                gui.ellipse(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 2], hitboxData[i + 3])
            end
        else
            table.insert(gameInfo, line)
        end
    end

    drawGameInfo(gameInfo)
end

function drawGameInfo(textArray)
    local screenWidth, screenHeight = gui.resolution()
    for i, v in ipairs(textArray) do
        gui.text(screenWidth, 23 * (i - 1), v)
    end
end

function readString(address)
    local text = {}
    local len = memory.readu16(address + 0x10)
    for i = 1, len do
        text[i] = string.char(memory.readu16(address + 0x12 + i * 2))
    end
    return table.concat(text)
end

function splitString(text, sep)
    if sep == nil then
        sep = "%s"
    end
    local t = {}
    for str in string.gmatch(text, "([^" .. sep .. "]+)") do
        table.insert(t, str)
    end
    return t
end

function getInfoAddress()
    local infoAddress = 0
    local markOffset = 0
    local matchedGameVersion = 0

    if gameVersion == nil or gameVersion == 1028 then
        infoAddress = getPointerAddress({ 0x400000 + 0x1B1CF60, 0x400, 0x18, 0x20 })
        markOffset = 0xF78
        matchedGameVersion = 1028
    end

    if gameVersion == nil or gameVersion == 1221 then
        infoAddress = getPointerAddress({ 0x400000 + 0x1B1CF60, 0x400, 0x18, 0x20 })
        markOffset = 0xF38
        matchedGameVersion = 1221
    end

    if gameVersion == nil or gameVersion == 1432 then
        infoAddress = getPointerAddress({ 0x400000 + 0x20E41A8, 0x38, 0x10, 0xA8, 0x8D8 })
        markOffset = 0x28
        matchedGameVersion = 1432
    end

    if infoAddress ~= 0 and memory.readu64(infoAddress + markOffset) == 1234567890123456789 then
        gameVersion = matchedGameVersion
        return memory.readu64(infoAddress + markOffset + 0x8)
    else
        return 0
    end
end

function getPointerAddress(offsets)
    local address = 0
    for i, v in ipairs(offsets) do
        address = memory.readu64(address + v)
        if address == 0 then
            return 0
        end
    end
    return address
end
