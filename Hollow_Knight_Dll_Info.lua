local showPos = true
local showVel = true
local showGameState = true
local showInGameTime = true
local showHp = true

function onPaint()
    local gameManagerClass = memory.readu64(0x400000 + 0x1B317A8)
    gameManagerClass = memory.readu64(0x400000 + 0x1B317A8)
    gameManagerClass = memory.readu64(gameManagerClass + 0x20)
    gameManagerClass = memory.readu64(gameManagerClass + 0xF0)
    gameManagerClass = memory.readu64(gameManagerClass + 0x8)
    gameManagerClass = memory.readu64(gameManagerClass + 0x18)

    local infoAddress = memory.readu64(gameManagerClass + 0x70)

    if infoAddress == 0 then
        return ;
    end

    local textArray = {}
    local infoText = readString(infoAddress)
    for line in infoText:gmatch("[^\r\n]+") do
        if line:find("^HP:") ~= nil then
            local hpData = splitString(line:sub(4), ",")
            for i = 1, #hpData, 3 do
                gui.text(hpData[i], hpData[i + 1], hpData[i + 2])
            end
        else
            table.insert(textArray, line)
        end
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