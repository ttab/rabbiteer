Rabbiteer
=========

.NET service + client to publish/consume messages to RabbitMQ

## Service

The service runs as a standard windows service. It maintains a persistent connection to RabbitMQ. The client issues commands to the server through standard windows RPC-mechanism. The server writes log messages to the windows event log `Application` with event source set to `Rabbiteer`.

The server has an internal state in memory which is lost when restarting (binds are not persistent).

## Client

Client is a CLI tool .exe usable from powershell or cmd.

### Bind command

The bind command tells the server to bind a (named) queue to a certain exchange with a specific routing key. Messages published to this exchange will be written to the provided outdir.

    rabbiteer -b -q delivery-automate -e delivery -r out.# -o c:\delivery\new
    
This binds the queue `delivery-automate` to the exchange `delivery` using routing key `out.#` and writes any messages received to `c:\delivery\new`.

#### File naming

Messages received are written as individual files. Each file will be named `yyyyMMdd-HHmmss`.`[extension for mime type]` unless an amqp message header `fileName` is provided, in which case this is used instead. When using `fileName` files named the same will overwrite each other.

The mime type is mapped according to a big internal list of mime type to file name extension mappings.

#### Repeatable

The bind command is repeatable. If the exact same parameters are reissued, no additional queue or listener will be set up. Rabbiteer service has a state only in memory and restarts resets the entire service. The intention is to reissue the bind commands on scheduled basis.

### Publish command

The publish commands tells the server process to send a file to a certain exchange and routing key and to move the file when it has been sent.

    rabbiteer -p -e ttnitf -r ttnitf -f myFile.ttt -d c:\done
    
This publishes the file `myFile.ttt` to the exchange `ttnitf` with the routing key `ttnitf` and moves the file to `c:\done` when finished.

Note that command line exe does not interact the file at all (it does check whether it exists), instead it instructs the service to do so. The actual file reading and moving is done by the rabbiteer service, ensure correct file permissions for the service.

The publish command will always send the `fileName` header having the file name. The mime type is mapped using an internal filename extension to mime type table and defaults to `application/octet-stream` if nothing good is found.

Only if the file was successfully sent to RabbitMQ is the file moved to the done dir.
