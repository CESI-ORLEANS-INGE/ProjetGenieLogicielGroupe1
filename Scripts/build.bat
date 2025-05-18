@echo off

@rem If the script is not run from the root of the project or from the Scripts folder
if not exist "*.sln" (
    cd ..
    if not exist "*.sln" (
        echo "Please run this script from the root of the project or from the Scripts folder."
        exit /b 1
    )
)

@rem Compile the project
@rem Windows 64-bit
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/easysave-win-x64

@rem Linux 64-bit
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/easysave-linux-x64

@rem macOS 64-bit
dotnet publish -c Release -r osx-x64 --self-contained true -o ./publish/easysave-osx-x64

@rem Create the zip files
@rem Windows 64-bit
powershell -Command "Compress-Archive -Path ./publish/easysave-win-x64/* -DestinationPath ./publish/easysave-win-x64.zip"

@rem Linux 64-bit
powershell -Command "Compress-Archive -Path ./publish/easysave-linux-x64/* -DestinationPath ./publish/easysave-linux-x64.zip"

@rem macOS 64-bit
powershell -Command "Compress-Archive -Path ./publish/easysave-osx-x64/* -DestinationPath ./publish/easysave-osx-x64.zip"