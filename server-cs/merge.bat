@ECHO OFF
IF "%1"=="Debug" GOTO END
PUSHD %~dp0
	REM SET BASE_FOLDER=SimpleMassiveRealtimeRankingServer\bin\Release
	SET BASE_FOLDER=SimpleMassiveRealtimeRankingServer\bin\Release
	SET FILES=
	SET FILES=%FILES% %BASE_FOLDER%\SimpleMassiveRealtimeRankingServer.exe
	SET FILES=%FILES% %BASE_FOLDER%\CSharpUtils.dll

	SET TARGET=/targetplatform:v4,"%ProgramFiles%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
	"C:\Program Files (x86)\Microsoft\ILMerge\ilmerge.exe" %TARGET% /out:SimpleMassiveRealtimeRankingServer.exe %FILES%
POPD
:END