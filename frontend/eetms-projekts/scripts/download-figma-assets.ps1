param(
    [string]$outputDir = "public/assets"
)

# Create output directory if it doesn't exist
if (-not (Test-Path $outputDir)) { New-Item -ItemType Directory -Path $outputDir | Out-Null }

$assets = @(
    @{url='https://www.figma.com/api/mcp/asset/1dfa32d1-51f5-45cd-b5be-69e33c1688fd'; name='imgEllipse1.png'},
    @{url='https://www.figma.com/api/mcp/asset/f816f1a8-7989-4ba3-8360-bdf8419aafd8'; name='imgEllipse2.png'},
    @{url='https://www.figma.com/api/mcp/asset/e7b2ec09-f202-43bf-92fe-136ae96ea752'; name='img.png'},
    @{url='https://www.figma.com/api/mcp/asset/5b95b9d3-6c40-4ff1-93f6-0575729cf640'; name='imgVector4.png'},
    @{url='https://www.figma.com/api/mcp/asset/331df752-fd51-45cc-82b9-c42667aa6b9c'; name='imgVector6.png'},
    @{url='https://www.figma.com/api/mcp/asset/475c97ea-f5b2-4501-bfc1-40dae0f6bce4'; name='imgVector8.png'},
    @{url='https://www.figma.com/api/mcp/asset/2a7011f1-6161-4516-8598-6d7d728dc2b8'; name='imgVector5.png'},
    @{url='https://www.figma.com/api/mcp/asset/25a8eb27-8fa2-42c5-894a-aa2e3e853429'; name='imgVector7.png'},
    @{url='https://www.figma.com/api/mcp/asset/98ca9e26-5919-4f79-b4bb-53272a9575c5'; name='imgVector9.png'},
    @{url='https://www.figma.com/api/mcp/asset/9b457114-a5f8-4f0d-8439-06624e37b1a5'; name='imgVector13.png'},
    @{url='https://www.figma.com/api/mcp/asset/c3388713-1354-4f17-b764-31ba6676bca7'; name='imgVector12.png'},
    @{url='https://www.figma.com/api/mcp/asset/084cfaf8-64db-4ea5-b349-eb329184a3dd'; name='imgEllipse4.png'},
    @{url='https://www.figma.com/api/mcp/asset/a3c9c746-454f-4191-ab5b-12ca5d4001fa'; name='imgEllipse7.png'},
    @{url='https://www.figma.com/api/mcp/asset/be578972-9306-4b02-b216-91c1b955eb0a'; name='imgVector10.png'},
    @{url='https://www.figma.com/api/mcp/asset/17ea3433-63d4-4f2c-9666-03be19165721'; name='imgVector11.png'},
    @{url='https://www.figma.com/api/mcp/asset/5072f549-bf38-4924-9f07-3b96dc111eec'; name='imgVector14.png'},
    @{url='https://www.figma.com/api/mcp/asset/d293e776-510a-4a29-88b1-4e90d162d530'; name='img1.png'},
    @{url='https://www.figma.com/api/mcp/asset/fd47e656-13be-4101-b59d-9af7de98d1d6'; name='img2.png'},
    @{url='https://www.figma.com/api/mcp/asset/16a401b7-7668-4abc-a289-ef724bbc6bb9'; name='img3.png'},
    @{url='https://www.figma.com/api/mcp/asset/d898aa00-ce64-46b3-884c-04836f6bd8ee'; name='img4.png'},
    @{url='https://www.figma.com/api/mcp/asset/a24d20c9-9a93-43fc-a5ef-fb78b2d0e7bc'; name='img5.png'},
    @{url='https://www.figma.com/api/mcp/asset/f7850ff1-9b4c-4078-afbb-ecc86f6a8955'; name='img6.png'},
    @{url='https://www.figma.com/api/mcp/asset/6edaf521-ece4-44b3-8969-7aaa6acb6f0c'; name='img7.png'},
    @{url='https://www.figma.com/api/mcp/asset/504b26e6-d412-4315-8783-0828ba3af913'; name='img8.png'},
    @{url='https://www.figma.com/api/mcp/asset/141a39ab-cdd0-4bd7-bfb0-ca1e166e49ec'; name='img9.png'},
    @{url='https://www.figma.com/api/mcp/asset/e7b2ec09-f202-43bf-92fe-136ae96ea752'; name='rimi.png'}
)

foreach ($asset in $assets) {
    $url = $asset.url
    $name = $asset.name
    $out = Join-Path $outputDir $name
    Write-Host "Downloading $url -> $out"
    try {
        Invoke-WebRequest -Uri $url -OutFile $out -UseBasicParsing -ErrorAction Stop
    } catch {
        Write-Warning ("Failed to download {0}: {1}" -f $url, $_.Exception.Message)
    }
}

Write-Host "Done. Place committed files in your repo and reference from /assets/."
