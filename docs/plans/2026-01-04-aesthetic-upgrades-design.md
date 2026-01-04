# Barrow Weather Aesthetic Upgrades Design

## Overview

Visual refresh of the Barrow Weather app with a clean, modern look featuring sky blue accents, frosted glass cards, temperature-based color hints, and polished animations.

## Color System

### Primary Palette
- **Sky Blue Accent**: #0078D4 (buttons, icons, highlights)
- **Sky Blue Light**: #4BA0E8 (gradients, hover states)
- **Sky Blue Dark**: #005A9E (active states, emphasis)

### Background Gradients (condition-based)
| Condition | Start Color | End Color |
|-----------|-------------|-----------|
| Clear/Sunny | #1a3a5c | #4a7ba7 |
| Cloudy | #4a5568 | #718096 |
| Snow/Precipitation | #2d3748 | #4a5568 |
| Night | #1a202c | #2d3748 |

### Temperature Color Hints
| Temperature | Tint Color | Description |
|-------------|------------|-------------|
| Below 0°F | #E3F2FD | Icy blue |
| 0-32°F | #BBDEFB | Cool blue |
| Above 32°F | #FFF8E1 | Warm (rare for Barrow) |

### Card Styling
- Frosted glass effect (semi-transparent with blur)
- Drop shadows: `0 4px 12px rgba(0,0,0,0.15)`
- Corner radius: 12px

## Component Designs

### Current Conditions Card (Hero)
- Temperature: 96px font, sky blue accent color
- Frosted glass background with temperature-based tint
- Weather icons colorized (sun=gold, cloud=gray, snow=white)
- Subtle inner glow effect

### Header Area
- Location: Semibold with subtle text shadow
- Fresh data indicator: Pulsing green dot when < 5 min old
- Refresh button: Sky blue pill shape, white icon, rotates during refresh

### Forecast Cards
- Frosted glass styling (consistent with conditions card)
- Section headers: Sky blue accent underline
- Hourly: Current hour highlighted with accent background
- Daily: Today's row has accent left border
- High temps bold, low temps muted gray

### Alerts Card
- Keep caution yellow/orange theme
- Pulsing border animation for severe alerts
- Warning icon with subtle glow

## Animations

### On Load
- Staggered card fade-in (100ms delay between cards)
- Slide up 20px as they fade in
- Duration: 300ms, ease-out

### On Refresh
- Loading ring pulses with sky blue glow
- Cards fade to 50% opacity during load
- Smooth fade back on completion

### Interactive Elements
- Buttons: Scale 98% on press, shadow lift on hover
- Cards: Elevation increase on hover
- Refresh icon: 360° rotation during refresh

### Ambient
- Fresh data: Gentle pulsing green dot
- Severe alert: Slow border pulse (2s cycle)
- All transitions: 200ms ease

## Implementation

### New Files
- `Themes/WeatherStyles.xaml` - Central style definitions
- `Converters/TemperatureToColorConverter.cs` - Temperature-based tinting

### Modified Files
- `App.xaml` - Import resource dictionary
- `MainWindow.xaml` - Background gradient, header styling
- `MainWindow.xaml.cs` - Condition-based background logic
- `CurrentConditionsCard.xaml` - Hero styling
- `AlertsCard.xaml` - Pulsing animation
- `HourlyForecastCard.xaml` - Current hour highlight
- `DailyForecastCard.xaml` - Today highlight

### Out of Scope
- Custom weather icon pack (use Segoe MDL2, colorize)
- Complex weather animations (rain, snow particles)
- Animated background gradient shifts
