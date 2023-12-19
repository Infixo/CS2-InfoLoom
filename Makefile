all: build
BEPINEX_VERSION = 5

clean:
	@dotnet clean

restore:
	@dotnet restore

build-ui:
	@npm install
	@npx esbuild ui_src/demographics.jsx --bundle --outfile=dist/demographics.js
	@npx esbuild ui_src/workforce.jsx --bundle --outfile=dist/workforce.js
	@npx esbuild ui_src/demandfactors.jsx --bundle --outfile=dist/demandfactors.js

copy-ui:
	copy dist\demographics.js "C:\Steam\steamapps\common\Cities Skylines II\Cities2_Data\StreamingAssets\~UI~\HookUI\Extensions\panel.infoloom.demographics.js"
	copy dist\workforce.js "C:\Steam\steamapps\common\Cities Skylines II\Cities2_Data\StreamingAssets\~UI~\HookUI\Extensions\panel.infoloom.workforce.js"
	copy dist\demandfactors.js "C:\Steam\steamapps\common\Cities Skylines II\Cities2_Data\StreamingAssets\~UI~\HookUI\Extensions\panel.infoloom.demandfactors.js"

build: clean restore build-ui
	@dotnet build /p:BepInExVersion=$(BEPINEX_VERSION)

dev-demand:
	@npx esbuild ui_src/demandfactors.jsx --bundle --outfile=dist/demandfactors.js
	copy dist\demandfactors.js "C:\Steam\steamapps\common\Cities Skylines II\Cities2_Data\StreamingAssets\~UI~\HookUI\Extensions\panel.infoloom.demandfactors.js"
	
dev-workforce:
	@npx esbuild ui_src/workforce.jsx --bundle --outfile=dist/workforce.js
	copy dist\workforce.js "C:\Steam\steamapps\common\Cities Skylines II\Cities2_Data\StreamingAssets\~UI~\HookUI\Extensions\panel.infoloom.workforce.js"

dev-demo:
	@npx esbuild ui_src/demographics.jsx --bundle --outfile=dist/demographics.js
	copy dist\demographics.js "C:\Steam\steamapps\common\Cities Skylines II\Cities2_Data\StreamingAssets\~UI~\HookUI\Extensions\panel.infoloom.demographics.js"

package-win:
	@-mkdir dist
	@cmd /c copy /y "bin\Debug\netstandard2.1\0Harmony.dll" "dist\"
	@cmd /c copy /y "bin\Debug\netstandard2.1\PopStruct.dll" "dist\"
	@echo Packaged to dist/

package-unix: build
	@-mkdir dist
	@cp bin/Debug/netstandard2.1/0Harmony.dll dist
	@cp bin/Debug/netstandard2.1/PopStruct.dll dist
	@echo Packaged to dist/