# E2E Screenshot Baselines

Schvalene baseline screenshoty patri do stejne relativni struktury jako runtime artefakty:

`{oblast}/{scenar}/{viewport}/{theme}/{state}.png`

Cesta se zapise do `approved-screenshots.json` az po UX schvaleni. Dokud cesta v manifestu neni, `TakeCheckpointScreenshotAsync` pouze ulozi review artefakt do `artifacts/e2e/screenshots`. Jakmile cesta v manifestu je, helper navic zavola `ToHaveScreenshotAsync` a porovna aktualni screenshot se schvalenou baseline.
