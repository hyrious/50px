# 50px

Resize stickers before sending them. Personally use in QQ.

## Usage

### Offline

Install [ImageMagick](https://www.imagemagick.org/script/download.php) & [gifsicle](https://www.lcdf.org/gifsicle/) first. Then use them like this:

```bash
# normal image (not gif)
magick clipboard: -resize 50x-1 clipboard:
magick input.jpg -resize 50x-1 output.jpg
# gif
gifsicle --resize-fit-width 50 -i input.gif -o output.gif
# gif (ffmpeg) -> https://github.com/jauhc/ffmpeg-gif-resize
```

#### Windows

Download prebuilt binary from [releases](https://github.com/hyrious/50px/releases). This little script will listen to clipboard data and convert images automatically or manually then put them back to the clipboard. Also, it needs `magick` and `gifsicle` present in PATH.

### Online

[Please use the latest stable Chrome or Firefox to open this link.](https://hyrious.me/50px)

Currently, the Web API only allows us getting the image from `<input type=file>` instead of a clipboard.

## License

MIT.
