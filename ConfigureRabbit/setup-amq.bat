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
echo ::python ./rabbitmqadmin --help
echo ::python ./rabbitmqadmin --help subcommand
echo
call :colorEcho 03 "CREATE VIRTUAL HOST"
echo.
python .\rabbitmqadmin declare vhost name="test-vhost"
python .\rabbitmqadmin declare permission vhost="test-vhost" user="guest" configure=".*" write=".*" read=".*"

echo.
call :colorEcho 03 "Configuring - DIRECT EXCHANGE -"
echo.
python .\rabbitmqadmin declare exchange name="test.direct.exchange.v-1.0" type="direct" auto_delete=false durable=true internal=false
python .\rabbitmqadmin declare queue name="test.direct.q" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin declare binding source="test.direct.exchange.v-1.0" destination="test.direct.q" routing_key="test.direct.q"

echo.
call :colorEcho 03 "Configuring - VHOST DIRECT EXCHANGE -"
echo.
python .\rabbitmqadmin --vhost="test-vhost" declare exchange name="test.direct.exchange.v-1.0" type="direct" auto_delete=false durable=true internal=false
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.direct.q" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.direct.exchange.v-1.0" destination="test.direct.q" routing_key="test.direct.q"



echo.
call :colorEcho 03 "Configuring - TOPIC EXCHANGE -"
echo.
python .\rabbitmqadmin declare exchange name="test.topic.exchange.v-1.0" type="topic" auto_delete=false durable=true internal=false
python .\rabbitmqadmin declare queue name="test.topic.q.1" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.1" routing_key="topic.1"

python .\rabbitmqadmin declare queue name="test.topic.q.2" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.2" routing_key="topic.2"

echo.
call :colorEcho 03 "Configuring - VHOST TOPIC EXCHANGE -"
echo.
python .\rabbitmqadmin --vhost="test-vhost" declare exchange name="test.topic.exchange.v-1.0" type="topic" auto_delete=false durable=true internal=false
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.1" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.1" routing_key="topic.1"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.2" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.2" routing_key="topic.2"



echo.
call :colorEcho 03 "Configuring - FANOUT EXCHANGE -"
echo.
python .\rabbitmqadmin declare exchange name="test.fanout.exchange.v-1.0" type="fanout" auto_delete=false durable=true internal=false
python .\rabbitmqadmin declare queue name="test.fanout.q.1" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.1"
python .\rabbitmqadmin declare queue name="test.fanout.q.2" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.2"

echo.
call :colorEcho 03 "Configuring - VHOST FANOUT EXCHANGE -"
echo.
python .\rabbitmqadmin --vhost="test-vhost" declare exchange name="test.fanout.exchange.v-1.0" type="fanout" auto_delete=false durable=true internal=false
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.1" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.1"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.2" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.2"

goto :EOF

:colorEcho
echo off
<nul set /p ".=%DEL%" > "%~2"
findstr /v /a:%1 /R "^$" "%~2" nul
del "%~2" > nul 2>&1i
