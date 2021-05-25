function onPaint()
    local infoAddress = getInfoAddress()

    if infoAddress == 0 then
        return ;
    end

    local infoText = readString(infoAddress)
    local gameInfo = {}
    for line in infoText:gmatch("[^\r\n]+") do
        if line:find("^HP=") ~= nil then
            local hpData = splitString(line:sub(4), ",")
            for i = 1, #hpData, 3 do
                gui.text(hpData[i], hpData[i + 1], hpData[i + 2])
            end
        elseif line:find("^LineHitbox=") ~= nil then
            local hitboxData = splitString(line:sub(12), ",")
            for i = 1, #hitboxData, 5 do
                gui.line(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 3], hitboxData[i + 4])
            end
        elseif line:find("^CircleHitbox=") ~= nil then
            local hitboxData = splitString(line:sub(14), ",")
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
    return table.concat(text);
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
    local infoAddress = memory.readu64(0x400000 + 0x1B1CF60)
    infoAddress = memory.readu64(infoAddress + 0x400)
    infoAddress = memory.readu64(infoAddress + 0x18)
    infoAddress = memory.readu64(infoAddress + 0x20)

    if memory.readu64(infoAddress + 0xF38) == 1234567890 then
        -- v1221
        return memory.readu64(infoAddress + 0xF40)
    elseif memory.readu64(infoAddress + 0xF78) == 1234567890 then
        -- v1028
        return memory.readu64(infoAddress + 0xF80)
    else
        return 0;
    end
end 