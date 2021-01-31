# TemperatureInflux

This tool can be used to get information from [Open Hardware Monitor](https://openhardwaremonitor.org/) into influx.

Download the Repository, build and add the following into your `telegraf.conf`:

```toml
[[inputs.exec]]
  data_format = "influx"
  commands = ["PATH/TO/TemperatureInflux.exe",]

  ## Timeout for each command to complete.
  timeout = "5s"

  ## measurement name suffix (for separating different commands)
  #name_suffix = "_mycollector"
```

## Configure the output

To configure the output, simply update the values inside the `TemperatureInflux.exe.config`.

`ModuleName`: The name of the module for influx  

`EnabledModules`: Comma seperated list of modules which should be loaded (impacts performance). 
Allowed values are: `CPU`, `RAM`, `Mobo`, `GPU`, `HDD`, `Fan`

`Sensors`: Comma seperated list of sensors to output. To get all available sensors, run `.\TemperatureInflux.exe list`



This Tool started by using the Code from http://www.lattepanda.com/topic-f11t3004.html