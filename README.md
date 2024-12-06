# **PathResolver**

PathResolver is a powerful and extensible library for resolving and evaluating file system paths based on dynamic expressions. It supports advanced path traversal and pattern matching, making it suitable for applications like file searches, build systems, and custom path resolvers.

---

## **Features**

- **Advanced Wildcard Matching**:
  - Supports wildcards (`*` and `**`) for flexible directory and file searches.
- **Parent Traversal**:
  - Use `..` to navigate to the parent directory and `...` to recursively traverse all parent directories.
- **Exact Matching**:
  - Match specific directories by name.
- **Tagged Search**:
  - Identify directories containing a marker file or subdirectory.
- **Composable Expressions**:
  - Combine strategies for complex path resolutions, such as `...\\**\\kxd`.

---

## **Installation**

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/PathResolver.git
   ```
2. Build the project using your favorite IDE or CLI:
   ```bash
   dotnet build
   ```

---

## **Usage**

### **Basic Path Matching**
```csharp
var currentDir = "C:\\Test";
var expression = "SubDir1\\SubSubDir1";
var evaluator = new PathResolver(fileSystem: new MockFileSystem(currentDir, existingDirectories));
var result = evaluator.EvaluateExpression(expression);
Console.WriteLine(result); // Output: "C:\\Test\\SubDir1\\SubSubDir1"
```

### **Wildcard Search**
Find a directory named `kxd`:
```csharp
var expression = "...\\**\\kxd";
var result = evaluator.EvaluateExpression(expression);
// Output: "C:\\Test\\SubDir1\\SubSubDir1\\bin\\Debug\\kxd"
```

### **Tagged Search**
Find a directory containing `.marker` and a specific subpath:
```csharp
var expression = "**\\:tagged(.marker):\\bin\\Debug\\kxd";
var result = evaluator.EvaluateExpression(expression);
// Output: "C:\\Test\\SubDir2\\SubSubDir1\\bin\\Debug\\kxd"
```

---

## **Expression Syntax**

| Symbol       | Description                                                                 |
|--------------|-----------------------------------------------------------------------------|
| `*`          | Matches any single directory name.                                         |
| `**`         | Matches directories recursively.                                           |
| `..`         | Moves to the parent directory.                                             |
| `...`        | Recursively traverses all parent directories.                              |
| `:tagged(x)` | Matches directories containing a file or folder named `x`.                 |

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

---

## **License**

This project is licensed under the [MIT License](LICENSE).
