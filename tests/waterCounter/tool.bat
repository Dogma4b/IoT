@echo off

set programm=nodemcu-tool
set port=COM3
set cmd=%programm% -p %port%

goto :case-%1 2>nul || (
    echo fsinfo [options]             Show file system info ^(current files, memory usage^)
    echo run ^<file^>                   Executes an existing .lua or .lc file on NodeMCU
    echo upload [options] [files...]  Upload Files to NodeMCU ^(ESP8266^) target
    echo download ^<file^>              Download files from NodeMCU ^(ESP8266^) target
    echo remove ^<file^>                Removes a file from NodeMCU filesystem
    echo mkfs [options]               Format the SPIFFS filesystem - ALL FILES ARE REMOVED
    echo terminal [options]           Opens a Terminal connection to NodeMCU
    echo init                         Initialize a project-based Configuration ^(file^) within current directory
    echo devices [options]            Shows a list of all available NodeMCU Modules/Serial Devices
    echo reset [options]              Execute a Hard-Reset of the Module using DTR/RTS reset circuit
)

:case-ls
    %cmd% fsinfo
:case-run
    %cmd% %1
:case-upload
    %cmd% %1 %2 %3
:case-download
    %cmd% %1 %2
:case-remove
    %cmd% %1 %2
:case-mkfs
    %cmd% %1
:case-terminal
    %cmd% %1 %2
:case-init
    %cmd% %1
:case-devices
    %cmd% %1
:case-reset
    %cmd% %1
:case-help
    %programm% %2 --help