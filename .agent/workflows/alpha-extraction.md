---
description: Create transparent PNG from AI-generated images using two-pass alpha extraction
---

# Two-Pass Alpha Extraction Workflow

This workflow creates transparent PNG images by generating the same subject on white and black backgrounds, then extracting the alpha channel.

## Prerequisites

- .NET 10 SDK installed
- pngalpha project built (`d:\github\transparent-png\pngalpha`)

## Steps

### 1. Generate Base Image on Neutral Background

First, generate your subject on a solid color background that contrasts with the subject (avoid white/black).

Example colors: blue (#0066CC), green (#00CC66), purple (#6600CC)

```
Use generate_image tool with your subject description + "on a solid [color] background #[hex]"
```

### 2. Convert to White Background

Use the generated image as input and change only the background to white.

```
Use generate_image tool with:
- ImagePaths: [path to base image from step 1]
- Prompt: "Change ONLY the background color to pure solid white #FFFFFF. Keep the subject exactly the same - same position, same shape, same style. Only replace the [color] background with pure white background."
```

### 3. Convert to Black Background

Use the white background image as input and change only the background to black.

```
Use generate_image tool with:
- ImagePaths: [path to white background image from step 2]
- Prompt: "Change ONLY the background color to pure solid black #000000. Keep the subject exactly the same - same position, same shape, same style. Only replace the white background with pure black background."
```

### 4. Extract Alpha Channel

Run the pngalpha tool to create the transparent PNG.

// turbo
```bash
cd d:\github\transparent-png\pngalpha
dotnet run -- "<white_background_image_path>" "<black_background_image_path>" "<output_transparent_png_path>"
```

> **PowerShell Hang Workaround**: The process might not return an exit code immediately even after finishing.
> Instead of waiting for `command_status`, use `find_by_name` or `list_dir` to check if `<output_transparent_png_path>` has been created.
> If the file exists, use `send_command_input` with `Terminate: true` to force close the process and proceed.

### 5. Verify Result

View the generated transparent PNG file to verify the alpha extraction worked correctly.

```
Use view_file tool on the output PNG path
```

## Notes

- The white and black background images must be pixel-aligned for best results
- AI image generation may introduce slight variations; results depend on consistency
- For production use, render the same 3D scene with different backgrounds for perfect alignment
