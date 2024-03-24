# Enter The Matrix In Console
Make your console enter the matrix!

![sample](https://github.com/NRG-Drink/matrix-like-code-rain/assets/123409068/0fd7b315-b394-493a-baba-2eb021cada74)


## Start With Defaults
```cmd
NRG.Matrix.App.exe
```

## Start With Parameters
```cmd
NRG.Matrix.App.exe --delay 80 --time "00:00:20" --maxobjects 100 --addrate "e => 2"
```
#### Delay
This will set a pause in milliseconds (ms) between the frames.
#### Time
This will set a time after which the matrix-animation is stopped.  
The time argument will also enter the benchmark mode with a performance summary at the end and a live count of the objects that are displayed. (object = char on screen)
#### Max-Objects
Will set the maximum number of objects. When the maximum number is reached, no more objects will be created. (object = char on screen)
#### Add-Rate
Any lambda function is valid `Func<int, float>`. Input is the current width of the window.
By default the window width is devided by 200 ("e => e / 200").
