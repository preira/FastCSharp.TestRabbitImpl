@echo off
SETLOCAL EnableDelayedExpansion
for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do (
set "DEL=%%a"
)


echo.
call :colorEcho 0a "TO GET HELP"
echo.
call :colorEcho 0a "USE "
echo.
call :colorEcho 0a "python ./rabbitmqadmin --help"
echo.
call :colorEcho 0a "python ./rabbitmqadmin --help subcommand"
echo.
echo.
call :colorEcho 0C "DELETE VIRTUAL HOST"
echo.
python .\rabbitmqadmin delete vhost name="test-vhost"

echo. 
call :colorEcho 0C "DELETE DIRECT EXCHANGE"
echo. 
python .\rabbitmqadmin delete exchange name="test.direct.exchange.v-1.0"
python .\rabbitmqadmin delete queue name="test.direct.q"

echo. 
call :colorEcho 0C "DELETE TOPIC EXCHANGE"
echo. 
python .\rabbitmqadmin delete exchange name="test.topic.exchange.v-1.0"
python .\rabbitmqadmin delete queue name="test.topic.q.1"
python .\rabbitmqadmin delete queue name="test.topic.q.2"

echo. 
call :colorEcho 0C "DELETE FANOUT EXCHANGE"
echo. 
python .\rabbitmqadmin delete exchange name="test.fanout.exchange.v-1.0"
python .\rabbitmqadmin delete queue name="test.fanout.q.1" 
python .\rabbitmqadmin delete queue name="test.fanout.q.2" 

goto :EOF

:colorEcho
echo off
<nul set /p ".=%DEL%" > "%~2"
findstr /v /a:%1 /R "^$" "%~2" nul
del "%~2" > nul 2>&1i
