# Enter The Matrix In Console
Make your console enter the matrix!

![sample](https://github.com/NRG-Drink/matrix-like-code-rain/assets/123409068/0fd7b315-b394-493a-baba-2eb021cada74)

## Install
```cmd
dotnet tool install --global NRG.Matrix
```

## Start With Defaults
```cmd
matrix.enter
```

## Start With Parameters
```cmd
matrix.enter --delay 80 --add-rate 1 --max-objects 9999
```
#### Delay
This will set a pause in milliseconds (ms) between the frames.
#### Max-Objects
Will set the maximum number of objects. When the maximum number is reached, no more objects will be created. (object = falling drop)
#### Add-Rate
A factor to a function who depends on screen width. It has impact on the number of objects that are added to the screen on each frame.
