# FluentUI
A Fluent-style WinForms control library based on .NET Framework 4.8.

## Design Principles
1. Lighting Effects
- Using shadows to represent depth
- Highlights to show interactive states
2. Depth
- Z-axis hierarchy
- Card layout
3. Motion
- Smooth transitions
- Natural animation curves
4. Material
- Acrylic effect
- Semi-transparent blur
5. Scale
- Responsive layout
- Adaptive DPI

## Installation
```powershell
dotnet add package FluentControls
```

## Updates
> FluentControls follows Semantic Versioning.  
> Breaking changes will only be introduced in major releases.

## Control List (26)

### Control Base Classes (2)
- FluentControlBase: The base class for all FluentControls controls, providing basic styles and behaviors.
- FluentContainerBase: The base class for container controls, supporting child control layout and management.

### Basic Controls (12)
- FluentButton: A button control supporting lighting effects and animations. 
- FluentTextBox: A text input box supporting input type control and prefixes/suffixes. 
- FluentLabel: A label control supporting multiple display modes. 
- FluentCheckBox: A checkbox control supporting custom styles. 
- FluentRadio: A radio button control supporting group management. 
- FluentComboBox: A combo box control supporting dropdown selection and search functionality.
- FluentSlider: A slider control that supports sliding selection and animation effects.
- FluentProgress: A progress bar control that supports various styles.
- FluentDateTimePicker: A control that supports date and time selection.
- FluentColorPicker: A color picker control that supports color selection and preset color pickers.
- FluentColorComboBox: A combo box control that supports color selection.
- FluentShape: A graphic control that supports drawing various shapes.

### Container Controls (2)
- FluentPanel: A panel control that supports various layout methods.
- FluentTabControl: A tab control that supports switching between multiple tabs. 

### List Controls (4)
- FluentCheckBoxList: A checkbox list control that supports multi-selection lists. 
- FluentRadioList: A radio button list control that supports single-selection lists. 
- FluentListBox: A list box control that supports multi-selection and custom styles.
- FluentTreeView: A tree view control that supports displaying a hierarchical structure.

### Data Controls (1)
- FluentDataGridView: A data grid view control that supports data binding and editing. (new)

### Layout Controls (1)
- FluentLayoutContainer: A layout container control that supports multiple layout methods. 

### Toolbar Controls (3)
- FluentToolStrip: A toolbar control that supports custom styles and behaviors. 
- FluentMenuStrip: A menu bar control that supports multi-level menu display. 
- FluentStatusStrip: A status bar control that supports status display. 

### Form Controls (2)
- FluentForm: A form control that supports custom styles and behaviors.
- FluentDialog: A dialog box control that supports multiple display styles. 

### Combo Controls (1)
- FluentPagination: A pagination control that supports pagination display and navigation. (new)

## Links
[Nuget](https://www.nuget.org/packages/FluentControls/)
