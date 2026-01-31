# pngalpha

Two-pass alpha extraction tool for creating transparent PNG images.

흰색/검정색 배경 이미지 쌍에서 투명 PNG를 생성하는 도구입니다.

---

## Usage / 사용법

```bash
pngalpha <image_on_white> <image_on_black> <output_file>
```

### Example / 예시

```bash
# Run with dotnet / dotnet으로 실행
dotnet run -- white.png black.png output.png

# Or use compiled executable / 또는 컴파일된 실행 파일 사용
pngalpha.exe white.png black.png output.png
```

### Arguments / 인자

| Argument | Description | 설명 |
|----------|-------------|------|
| `image_on_white` | Image captured on white background | 흰색 배경에서 촬영한 이미지 |
| `image_on_black` | Image captured on black background | 검정색 배경에서 촬영한 이미지 |
| `output_file` | Output transparent PNG file | 출력될 투명 PNG 파일 |

---

## Algorithm / 알고리즘

This tool uses a **two-pass alpha extraction** technique to recover transparency from two images of the same subject photographed against white and black backgrounds.

이 도구는 **투-패스 알파 추출** 기법을 사용하여 흰색과 검정색 배경에서 촬영한 동일 피사체의 두 이미지로부터 투명도를 복원합니다.

### Alpha Calculation / 알파 계산

The alpha value is calculated based on the color distance between corresponding pixels in both images.

알파 값은 두 이미지에서 대응하는 픽셀 간의 색상 거리를 기반으로 계산됩니다.

```
pixelDist = √[(Rw - Rb)² + (Gw - Gb)² + (Bw - Bb)²]
bgDist = √(255² + 255² + 255²) ≈ 441.67

alpha = 1 - (pixelDist / bgDist)
```

Where / 여기서:
- `(Rw, Gw, Bw)` = RGB values from white background image / 흰 배경 이미지의 RGB 값
- `(Rb, Gb, Bb)` = RGB values from black background image / 검정 배경 이미지의 RGB 값
- `bgDist` = Maximum possible distance (white to black) / 최대 가능 거리 (흰색에서 검정색)

### Principle / 원리

| Pixel Type | White BG | Black BG | Distance | Alpha |
|------------|----------|----------|----------|-------|
| **Opaque** (불투명) | Same color | Same color | 0 | 1.0 |
| **Transparent** (투명) | White (255,255,255) | Black (0,0,0) | 441.67 | 0.0 |
| **Semi-transparent** (반투명) | Blended with white | Blended with black | 0 < d < 441.67 | 0 < α < 1 |

- **Opaque pixels** appear identical on both backgrounds → distance = 0 → alpha = 1
- **Transparent pixels** show the background color → distance = max → alpha = 0
- **Semi-transparent pixels** show partial blending → proportional alpha

- **불투명 픽셀**은 두 배경에서 동일하게 보임 → 거리 = 0 → 알파 = 1
- **투명 픽셀**은 배경색을 그대로 보여줌 → 거리 = 최대 → 알파 = 0
- **반투명 픽셀**은 부분적으로 섞임 → 비례적인 알파 값

### Color Recovery / 색상 복원

Once alpha is calculated, the original foreground color is recovered by un-premultiplying:

알파가 계산되면, 원본 전경색은 프리멀티플라이 해제를 통해 복원됩니다:

```
R_out = R_black / alpha
G_out = G_black / alpha
B_out = B_black / alpha
```

This works because on a black background (0,0,0), the observed color is:

이것이 작동하는 이유는 검정 배경(0,0,0)에서 관찰되는 색상이 다음과 같기 때문입니다:

```
C_observed = C_foreground × alpha + C_background × (1 - alpha)
C_observed = C_foreground × alpha + 0 × (1 - alpha)
C_observed = C_foreground × alpha

Therefore / 따라서:
C_foreground = C_observed / alpha
```

---

## Build / 빌드

```bash
# Debug build / 디버그 빌드
dotnet build

# Release build / 릴리스 빌드
dotnet build -c Release

# Self-contained executable / 독립 실행 파일
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## Requirements / 요구사항

- .NET 10.0 SDK
- SixLabors.ImageSharp 3.1.12

---

## Workflow with AI Image Generation / AI 이미지 생성 워크플로우

When using AI image generation tools, follow this workflow to create transparent PNGs:

AI 이미지 생성 도구를 사용할 때, 다음 워크플로우를 따라 투명 PNG를 생성하세요:

### Step 1: Generate Base Image / 기본 이미지 생성

Generate your subject on a **neutral solid color background** (not white or black) that contrasts with the subject.

피사체와 대비되는 **중간색 단색 배경** (흰색/검정색 제외)에 생성합니다.

```
Example colors / 예시 색상: Blue #0066CC, Green #00CC66, Purple #6600CC
```

### Step 2: Convert to White Background / 흰색 배경으로 변환

Use the base image as reference and change **only the background** to pure white.

기본 이미지를 참조로 사용하고 **배경만** 순수 흰색으로 변경합니다.

### Step 3: Convert to Black Background / 검정색 배경으로 변환

Use the white background image as reference and change **only the background** to pure black.

흰색 배경 이미지를 참조로 사용하고 **배경만** 순수 검정색으로 변경합니다.

### Step 4: Extract Alpha / 알파 추출

Run pngalpha with the white and black background images.

흰색과 검정색 배경 이미지로 pngalpha를 실행합니다.

```bash
pngalpha white.png black.png transparent_output.png
```

### Workflow Diagram / 워크플로우 다이어그램

```
[Base Image]     [White BG]      [Black BG]      [Transparent PNG]
    🔵      →       ⚪       →       ⚫       →         🔲
 (neutral)      (reference)     (reference)       (final output)
```

> **Note / 참고**: AI image generation may introduce slight variations between images, which can cause artifacts in the final result. For best results, use 3D rendering software or physical photography with fixed camera positioning.
>
> AI 이미지 생성은 이미지 간에 약간의 변형이 발생할 수 있어 최종 결과에 아티팩트가 생길 수 있습니다. 최상의 결과를 위해 3D 렌더링 소프트웨어 또는 카메라 고정 실물 촬영을 사용하세요.

---

## Tips / 팁

1. **Image alignment is critical** - Both images must be pixel-perfectly aligned.
   
   **이미지 정렬이 중요합니다** - 두 이미지는 픽셀 단위로 완벽하게 정렬되어야 합니다.

2. **Use even lighting** - Avoid shadows or reflections that differ between shots.
   
   **균일한 조명 사용** - 촬영 간에 다른 그림자나 반사를 피하세요.


3. **Camera must be fixed** - Use a tripod to ensure identical framing.
   
   **카메라 고정 필수** - 삼각대를 사용하여 동일한 프레이밍을 보장하세요.

4. **For rendered images** - Simply export the same scene with white and black backgrounds.
   
   **렌더링 이미지의 경우** - 동일한 장면을 흰색과 검정색 배경으로 각각 내보내세요.

---

## Example / 예시

The `examples/` folder contains a complete workflow demonstration using a magic potion bottle:

`examples/` 폴더에 마법 포션 병을 사용한 완전한 워크플로우 예시가 포함되어 있습니다:

| Step | File | Description | 설명 |
|------|------|-------------|------|
| 1 | `01_potion_base.png` | Base image on purple background | 보라색 배경의 기본 이미지 |
| 2 | `02_potion_white.png` | Converted to white background | 흰색 배경으로 변환 |
| 3 | `03_potion_black.png` | Converted to black background | 검정색 배경으로 변환 |
| 4 | `04_potion_transparent.png` | Final transparent PNG | 최종 투명 PNG |

### Running the Example / 예시 실행

```bash
cd pngalpha
dotnet run -- examples/02_potion_white.png examples/03_potion_black.png examples/output.png
```
