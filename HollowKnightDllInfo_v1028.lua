-------------- config --------------
local showHP = true
local showHitbox = true

local hitboxColors = {
    Knight = 0xFF00EE00,
    Enemy = 0xFFEE0000,
    PeaceMonster = 0xFFDDAA10,
    Trigger = 0xFF9370DB,
    Terrain = 0xFFFF8040,
}
-------------- config --------------

-- 屏幕边缘无法绘制的像素
local screenEdge = 6
local screenWidth = 0
local screenHeight = 0

function onStart()
    screenWidth, screenHeight = gui.resolution()
    screenWidth = screenWidth - screenEdge;
    screenHeight = screenHeight - screenEdge;
end

function onPaint()
    if screenWidth == 0 then
        onStart()
    end

    local infoAddress = getInfoAddress1028()

    if infoAddress == 0 then
        return ;
    end

    local infoText = readString(infoAddress)
    local gameInfo = {}
    for line in infoText:gmatch("[^\r\n]+") do
        if line:find("^HP=") ~= nil then
            if showHP then
                local hpData = splitString(line:sub(4), ",")
                for i = 1, #hpData, 3 do
                    gui.text(hpData[i], hpData[i + 1], hpData[i + 2])
                end
            end
        elseif line:find("^LineHitbox=") ~= nil then
            if showHitbox then
                local hitboxData = splitString(line:sub(12), ",")
                for i = 1, #hitboxData, 5 do
                    gui.line(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 3], hitboxColors[hitboxData[i + 4]])
                end
            end
        elseif line:find("^CircleHitbox=") ~= nil then
            if showHitbox then
                local hitboxData = splitString(line:sub(14), ",")
                for i = 1, #hitboxData, 4 do
                    gui.ellipse(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 2], hitboxColors[hitboxData[i + 3]])
                end
            end
        else
            table.insert(gameInfo, line)
        end
    end

    drawGameInfo(gameInfo)
end

function clamp(v, minValue, maxValue)
    if v < minValue then
        return minValue
    end
    if (v > maxValue) then
        return maxValue
    end
    return v
end

function outOfWidth(x)
    return x < screenEdge or x > screenWidth
end

function outOfHeight(y)
    return y < screenEdge or y > screenHeight
end

function outOfScreen(x, y)
    return x < screenEdge or y < screenEdge or x > screenWidth or y > screenHeight
end

function clampPoint(x, y)
    x = clamp(x, screenEdge, screenWidth)
    y = clamp(y, screenEdge, screenHeight)
    return x, y
end

function drawGameInfo(textArray)
    for i, v in ipairs(textArray) do
        gui.text(screenWidth, 23 * (i - 1), v)
    end
end

function drawPoint(x, y, color)
    x = tonumber(x)
    y = tonumber(y)
    if outOfScreen(x, y) then
        return
    end
    gui.pixel(x, y, color)
end

function drawHollowRect(x, y, width, height, thickness, color)
    x = tonumber(x)
    y = tonumber(y)

    drawRect(x, y, width, thickness, color)
    drawRect(x, y + height - thickness, width, thickness, color)
    drawRect(x, y, thickness, height, color)
    drawRect(x + width - thickness, y, thickness, height, color)
end

function drawRect(left, top, width, height, color)
    left = tonumber(left)
    top = tonumber(top)

    local right = left + width
    local bottom = top + height

    if left > screenWidth or right < screenEdge or top > screenHeight or bottom < screenEdge then
        return
    end

    left = clamp(left, screenEdge, screenWidth)
    right = clamp(right, screenEdge, screenWidth)
    top = clamp(top, screenEdge, screenHeight)
    bottom = clamp(bottom, screenEdge, screenHeight)

    gui.rectangle(left, top, right - left, bottom - top, 0, color, color)
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

function getInfoAddress1028()
    local infoAddress = memory.readu64(0x400000 + 0x1B1CF60)
    infoAddress = memory.readu64(infoAddress + 0x400)
    infoAddress = memory.readu64(infoAddress + 0x18)
    infoAddress = memory.readu64(infoAddress + 0x20)
    return memory.readu64(infoAddress + 0xF80)
end

function getInfoAddress1221()
    local infoAddress = memory.readu64(0x400000 + 0x1B317A8)
    infoAddress = memory.readu64(infoAddress + 0x20)
    infoAddress = memory.readu64(infoAddress + 0xF0)
    infoAddress = memory.readu64(infoAddress + 0x8)
    infoAddress = memory.readu64(infoAddress + 0x18)
    for i = 4, 10 do
        if memory.readu64(infoAddress + i * 0x20 + 0x8) == 1234567890 then
            return memory.readu64(infoAddress + i * 0x20 + 0x10)
        end
    end
    return 0;
end