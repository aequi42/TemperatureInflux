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


## Future Plans

Make the output more configurable... currently everything is hard coded for my needs.

This Tool started by using the Code from http://www.lattepanda.com/topic-f11t3004.html