local ActorStates = {
    "grounded",
    "idle",
    "running",
    "airborne",
    "wall_sliding",
    "hard_landing",
    "dash_landing",
    "no_input",
    "previous"
}

function onPaint()
    local baseAddress = 0x400000 + 0x1B31468
    local gameManager = memory.readu64(baseAddress)
    gameManager = memory.readu64(gameManager)
    gameManager = memory.readu64(gameManager + 0x10)
    gameManager = memory.readu64(gameManager + 0x20)
    gameManager = memory.readu64(gameManager + 0x18)

    if gameManager == 0 then
        return
    end

    local textArray = {}

    local heroController = memory.readu64(gameManager + 0x100)

    if heroController > 0 then
        local positionHistory = memory.readu64(heroController + 0x280)
        local positionX = memory.readf(positionHistory + 0x20)
        local positionY = memory.readf(positionHistory + 0x24)
        table.insert(textArray, "pos: " .. string.format("%.6f", positionX) .. ", " .. string.format("%.6f", positionY))

        local velocityX = memory.readf(heroController + 0x594)
        local velocityY = memory.readf(heroController + 0x598)
        table.insert(textArray, "vel: " .. string.format("%.3f", velocityX) .. ", " .. string.format("%.3f", velocityY))

        local heroState = memory.readu32(heroController + 0x578)
        table.insert(textArray, ActorStates[heroState + 1])
    end

    drawGameInfo(textArray)
end

function drawGameInfo(textArray)
    local width, _ = gui.resolution()
    for i, v in ipairs(textArray) do
        gui.text(width, 23 * (i - 1), v)
    end
end