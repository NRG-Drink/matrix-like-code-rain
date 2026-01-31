# Enter the Matrix in your console
Make your console enter the Matrix.

![sample gif](./docs/attachments/sample.gif)

## How to enter the Matrix
1. Install the tool

   ```cmd
   dotnet tool install --global NRG.Matrix
   ```
2. Open your terminal and run:

   ```cmd
   matrix.enter
   ```

> [!IMPORTANT]
> - Requires a terminal that supports ANSI escape sequences (Windows Terminal recommended)
> - Requires .NET 10 runtime (this tool targets `net10.0`)


## Update
```cmd
dotnet tool update --global NRG.Matrix
```

## Uninstall
```cmd
dotnet tool uninstall --global NRG.Matrix
```

## Usage

### Exit
Press `CTRL + C` to exit.

### Statistics `S`
Press `S` to show the statistics panel in the top-right corner.

### Controls `C`
Press `C` to show the controls-help menu in the bottom left corner.

`SHIFT + Arrows` to adjust object falling speed  
`CTRL + Arrows` to adjust object generation speed  
`CTRL + C` to exit the program

## Troubleshooting
- **Garbled output / missing colors:** Use a terminal with ANSI support (Windows Terminal recommended).
- **Tool command not found:** Restart your terminal after installation, and verify with `dotnet tool list --global`.

## Contributing
See [docs/how-to-develop.md](docs/how-to-develop.md).

## License
MIT. See [LICENSE](LICENSE).

## Spread some love
Star the repo or share it with your friends.