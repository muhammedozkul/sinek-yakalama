**English** · [Türkçe](README.tr.md)

# Sinek Yakalama (Catch the Fly)

A fly-swatting game made with C# Windows Forms for a Visual Programming course assignment. All graphics are drawn in code with GDI+.

## How to play
- Press the button to start a 30-second round.
- Click on flies to swat them with the racket: each fly is **+1** point.
- A **golden fly** shows up now and then for a short time: **+3** points.
- The higher your score, the faster the flies get. At the end you see your score, hits, misses and record.

## Running it
- **Windows:** open `gorsel programlama 2 - ders1 - sinek yakalama.slnx` in Visual Studio and press F5 (.NET Framework 4.7.2).
- **Quick look without Windows:** open `tarayici-onizleme.html` in a browser.

## Files
- `Form1.cs` — game logic, fly movement, mouse input and all drawing (sky, hills, flowers, flies)
- `Form1.Designer.cs` — form and button layout
- `Program.cs` — entry point
