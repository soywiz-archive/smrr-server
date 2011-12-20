@ECHO OFF
IF "%1"=="Debug" GOTO END
PUSHD %~dp0
	SET BASE_FOLDER=SimpleMassiveRealtimeRankingServer\bin\Release
	REM SET BASE_FOLDER=SimpleMassiveRealtimeRankingServer\bin\Debug
	SET FILES=
	SET FILES=%FILES% %BASE_FOLDER%\SimpleMassiveRealtimeRankingServer.exe
	REM SET FILES=%FILES% %BASE_FOLDER%\CSharpUtils.dll

	IF "%ProgramFiles(x86)%"=="" (
		SET TARGET=/targetplatform:v4,"%ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
	) ELSE (
		SET TARGET=/targetplatform:v4,"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
	)
	"C:\Program Files (x86)\Microsoft\ILMerge\ilmerge.exe" %TARGET% /out:SimpleMassiveRealtimeRankingServer.exe %FILES%
POPD
:END