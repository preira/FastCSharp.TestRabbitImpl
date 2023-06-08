python .\rabbitmqadmin declare exchange name="test.direct.exchange.v-1.0" type="direct" auto_delete=false durable=true internal=false
python .\rabbitmqadmin declare queue name="test.direct.q" queue_type=quorum auto_delete=false durable=true
python .\rabbitmqadmin declare binding source="test.direct.exchange.v-1.0" destination="test.direct.q" routing_key="test.direct.q"
