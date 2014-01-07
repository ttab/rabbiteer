
## Bind

    Rabbiteer.EXE -bind
                  -queue <qname>
                  -exchange <exname>
                  -topic <topicstring>
                  -out <outdir>

1. Does nothing if everything is already set up.
2. Declares/creates a queue named <qname> with options durable:true, autoDelete:false
3. Declares exchange named <exname> with options passive:true
4. Binds the queue to exchange with <topicstring>
5. Attaches a handler to queue which writes any incoming messages to directory <outdir>

Note:

* Requires a mapping from message mime-type to file name extension.
* For now we only support passive declaration of exchanges.

## Publish

    Rabbiteer.EXE -publish
                  -exchange <exname>
                  -topic <topicstring>
                  [-header <key>=<val>]
                  -file <file>

1. Declares exchange named <exname> with options passive:true
2. Creates message with correct contentType/encoding given file (assume `utf-8`)
3. Appends any provided -header switches to message. Notice that -header can be repeated
4. Publishes message to exchange.

Note:

* Requires mapping from file name extension to mime-type.
* Assumes `utf-8` when encoding is not explicit in the file (XML, HTML, etc).
* `-header` is optional and possible to repeat many times for additional headers.
* For now we only support passive declaration of exchanges.
