-------------- config --------------
local showHP = true
local showHitbox = true
local showPolygonHitbox = true
local hidePolygonHitboxWhilePlaying = false

local hitboxColors = {
    Knight = 0x0000EE00,
    Enemy = 0x00EE0000,
    PeaceMonster = 0x00DDAA10,
    Trigger = 0x009370DB,
    Terrain = 0x00FF8040,
}
-------------- config --------------

-- const
local moviePlayback = 2

-- 屏幕边缘无法绘制的像素
local screenEdge = 6
local screenWidth = 0
local screenHeight = 0

function init()
    if screenWidth == 0 then
        screenWidth, screenHeight = gui.resolution()
        screenWidth = screenWidth - screenEdge;
        screenHeight = screenHeight - screenEdge;
    end
end

function onPaint()
    init()

    local infoAddress = getInfoAddress1221()

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
        elseif line:find("^BoxHitbox=") ~= nil then
            if showHitbox then
                local hitboxData = splitString(line:sub(11), ",")
                for i = 1, #hitboxData, 5 do
                    drawHollowRect(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 3], 1, hitboxColors[hitboxData[i + 4]])
                end
            end
        elseif line:find("^EdgeHitbox=") ~= nil then
            if showHitbox then
                local hitboxData = splitString(line:sub(12), ",")
                for i = 1, #hitboxData, 5 do
                    drawRect(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 3], hitboxColors[hitboxData[i + 4]])
                end
            end
        elseif line:find("^PolyHitbox=") ~= nil then
            if showHitbox and showPolygonHitbox and not (hidePolygonHitboxWhilePlaying and movie.status() == moviePlayback) then
                local hitboxData = splitString(line:sub(12), ",")
                for i = 1, #hitboxData, 5 do
                    drawLine(hitboxData[i], hitboxData[i + 1], hitboxData[i + 2], hitboxData[i + 3], hitboxColors[hitboxData[i + 4]])
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

function drawLine(x0, y0, x1, y1, color)
    x0 = tonumber(x0)
    y0 = tonumber(y0)
    x1 = tonumber(x1)
    y1 = tonumber(y1)

    local dx = x1 - x0;
    local dy = y1 - y0;
    local stepx, stepy

    if dy < 0 then
        dy = -dy
        stepy = -1
    else
        stepy = 1
    end

    if dx < 0 then
        dx = -dx
        stepx = -1
    else
        stepx = 1
    end

    drawPoint(x0, y0, color)
    if dx > dy then
        local fraction = dy - math.floor(dx / 2.0)
        while x0 ~= x1 do
            if fraction >= 0 then
                y0 = y0 + stepy
                fraction = fraction - dx
            end
            x0 = x0 + stepx
            fraction = fraction + dy
            drawPoint(math.floor(x0), math.floor(y0), color)
        end
    else
        local fraction = dx - math.floor(dy / 2.0)
        while y0 ~= y1 do
            if fraction >= 0 then
                x0 = x0 + stepx
                fraction = fraction - dy
            end
            y0 = y0 + stepy
            fraction = fraction + dx
            drawPoint(math.floor(x0), math.floor(y0), color)
        end
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
    return memory.readu64(infoAddress + 0x90)
end