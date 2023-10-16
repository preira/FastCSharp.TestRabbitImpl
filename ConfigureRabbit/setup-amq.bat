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
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.1" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.2" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.2" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.3" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.3" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.4" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.4" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.5" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.5" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.6" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.6" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.7" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.7" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.8" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.8" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.9" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.9" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.10" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.10" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.11" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.11" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.12" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.12" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.13" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.13" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.14" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.14" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.15" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.15" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.16" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.16" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.17" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.17" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.18" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.18" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.19" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.19" routing_key="#"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.topic.q.20" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.topic.exchange.v-1.0" destination="test.topic.q.20" routing_key="#"



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
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.3" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.3"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.4" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.4"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.5" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.5"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.6" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.6"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.7" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.7"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.8" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.8"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.9" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.9"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.10" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.10"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.11" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.11"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.12" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.12"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.13" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.13"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.14" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.14"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.15" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.15"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.16" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.16"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.17" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.17"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.18" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.18"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.19" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.19"
python .\rabbitmqadmin --vhost="test-vhost" declare queue name="test.fanout.q.20" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin --vhost="test-vhost" declare binding source="test.fanout.exchange.v-1.0" destination="test.fanout.q.20"

goto :EOF

:colorEcho
echo off
<nul set /p ".=%DEL%" > "%~2"
findstr /v /a:%1 /R "^$" "%~2" nul
del "%~2" > nul 2>&1i
