<div align = "center">

# WildPath

![WildPath](resources/logo-transparent-256x256.png)


[![NuGet](https://img.shields.io/nuget/v/WildPath)](https://www.nuget.org/packages/WildPath/)
[![NuGet](https://img.shields.io/nuget/dt/WildPath)](https://www.nuget.org/packages/WildPath/)
[![License](https://img.shields.io/github/license/JKamsker/WildPath)](LICENSE)
[![PR Welcome](https://img.shields.io/badge/PR-Welcome-blue)](https://github.com/JKamsker/WildPath/pulls)

### .NET library for advanced file-system path expression resolution.

</div>

## **Features**

- **Advanced Wildcard Matching**:
  - Supports wildcards (`*` and `**`) for flexible directory and file searches. (e.g., `a\**\`).
- **Parent Traversal**:
  - Use `..` to navigate to the parent directory and `...` to recursively traverse all parent directories.
- **Exact Matching**:
  - Match specific directories by name.
- **Tagged Search**:
  - Identify directories containing a marker file or subdirectory. (e.g., `a\:tagged(.marker)\b`).
- **Composable Expressions**:
  - Combine strategies for complex path resolutions, such as `...\\**\\kxd`.
- **Plugins**:
  - Extend functionality with custom path resolvers and matchers.
  
### Showcase
![ezgif-4-c156370953](https://github.com/user-attachments/assets/0cd519b7-71a6-4cfd-8655-75c87c1b7628)

---

## **Installation**

WildPath is available as a NuGet package. You can install it using the .NET CLI or through the NuGet Package Manager in Visual Studio.

### **.NET CLI**

```bash
dotnet add package WildPath
```

### **Package Manager**

```bash
Install-Package WildPath
```

Alternatively, you can clone the repository and build the project manually:

1. Clone the repository:

```bash
git clone https://github.com/JKamsker/WildPath.git
```

2. Build the project using your favorite IDE or CLI:

```bash
dotnet build
```

For more details, visit the [NuGet package page](https://www.nuget.org/packages/WildPath).

---

## **Usage**

### **Basic Path Matching**

```csharp
using WildPath;

var result = PathResolver.Resolve("SubDir1\\SubSubDir1");
Console.WriteLine(result);
// Output (assuming current directory = "C:\\Test"): "C:\\Test\\SubDir1\\SubSubDir1"
```

### **Wildcard Search**

Find a directory named `kxd`:

```csharp
var result = PathResolver.Resolve("...\\**\\kxd");
// Output: "C:\\Test\\SubDir1\\SubSubDir1\\bin\\Debug\\kxd"
```

### **Tagged Search**

Find a directory containing `.marker` and a specific subpath:

```csharp
var result = PathResolver.Resolve("**\\:tagged(.marker):\\bin\\Debug");
// Output: "C:\\Test\\SubDir2\\SubSubDir1\\bin\\Debug"
```

### **Customized Path Resolution**

To use a custom **current directory**, **separator** or even a **different file system**, you can new up a `PathResolver` and use the same API:

```csharp
using WildPath;

var currentDir = "C:\\Test";
var expression = "SubDir1\\SubSubDir1";

var resolver = new PathResolver(currentDir);
var result = resolver.Resolve(expression);
Console.WriteLine(result); // Output: "C:\\Test\\SubDir1\\SubSubDir1"
```

### **Plugins**

[Here](https://github.com/JKamsker/WildPath/blob/master/WildPath.Console/Commands/Tui/TuiCommand.cs#L107) you can find an example of how to create and use a custom plugin.

---

## **Expression Syntax**

| Symbol       | Description                                                |
| ------------ | ---------------------------------------------------------- |
| `*`          | Matches any single directory name.                         |
| `**`         | Matches directories recursively.                           |
| `..`         | Moves to the parent directory.                             |
| `...`        | Recursively traverses all parent directories.              |
| `:tagged(x)` | Matches directories containing a file or folder named `x`. |

---

## **Testing**

Run the test suite to validate functionality:

```bash
dotnet test
```

Example tests include:

1. Wildcard and parent traversal.
2. Directory matching with markers.

---

## **Contributing**

1. Fork the repository.
2. Create a new branch:
   ```bash
   git checkout -b feature-name
   ```
3. Commit your changes and push to your fork:
   ```bash
   git commit -m "Description of feature"
   git push origin feature-name
   ```
4. Submit a pull request.

## **Todo**

### Limit recursive search depth:

- `...{1,3}\\**{1,3}\\:tagged(testhost.exe):\\fr` should only search 1-3 directories deep.
- `...{4}\\**{4}\\:tagged(testhost.exe):\\fr` should only search 4 directories deep.

### Add cancellation token support

Like seriously, this libaray begs for being ddosed.

## Not as serious todos:

### Search by file content:

Be a little cray cray

- `...\\**\\:content(test.json, test):` should search for files containing the string "test".

### Seach within zip files:

- `...\\**\\:zip(test.zip):\\fr` should search for files within a zip file named "test.zip".

---

## **License**

This project is licensed under the [MIT License](LICENSE).
