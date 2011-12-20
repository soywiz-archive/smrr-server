@echo off
PUSHD %~dp0
	rmdir /Q /S deploy
	del /Q deploy.7z
	mkdir deploy 2> NUL
	mkdir deploy\server 2> NUL
	mkdir deploy\client 2> NUL
	copy "%~dp0\server-cs\SimpleMassiveRealtimeRankingServer\bin\Debug\SimpleMassiveRealtimeRankingServer.exe" "deploy\server\SimpleMassiveRealtimeRankingServer.exe"
	copy "%~dp0\client\php\*.php" "deploy\client"
	PUSHD deploy
		"%ProgramFiles%\7-Zip\7z.exe" a ..\deploy.7z .
	POPD
POPD