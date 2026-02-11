using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.IconFonts
{
    public class ProjectResourceLocator
    {
        private ITypeDescriptorContext context;
        private IDesignerHost designerHost;

        public ProjectResourceLocator(ITypeDescriptorContext context)
        {
            this.context = context;
            this.designerHost = context?.GetService(typeof(IDesignerHost)) as IDesignerHost;
        }

        /// <summary>
        /// 获取项目根路径
        /// </summary>
        public string GetProjectPath()
        {
            // 方法1: 通过 RootComponentClassName 和反射
            var path = GetProjectPathFromClassName();
            if (!string.IsNullOrEmpty(path))
            {
                Debug.WriteLine($"[方法1] 通过类名获取路径: {path}");
                return path;
            }

            // 方法2: 通过 Component.Site 和设计器服务
            path = GetProjectPathFromSite();
            if (!string.IsNullOrEmpty(path))
            {
                Debug.WriteLine($"[方法3] 通过 Site 获取路径: {path}");
                return path;
            }

            // 方法3: 搜索文件系统
            path = SearchProjectPath();
            if (!string.IsNullOrEmpty(path))
            {
                Debug.WriteLine($"[方法4] 通过搜索获取路径: {path}");
                return path;
            }

            Debug.WriteLine("所有方法均失败，无法获取项目路径");
            return null;
        }

        /// <summary>
        /// 方法1: 通过 RootComponentClassName 获取
        /// </summary>
        private string GetProjectPathFromClassName()
        {
            try
            {
                if (designerHost == null)
                {
                    return null;
                }

                // 获取完整类名，如 "MyApp.Forms.MainForm"
                var fullClassName = designerHost.RootComponentClassName;

                Debug.WriteLine($"完整类名: {fullClassName}");

                if (string.IsNullOrEmpty(fullClassName))
                {
                    return null;
                }

                // 提取命名空间和类名
                string namespacePath = "";
                string simpleClassName = fullClassName;

                if (fullClassName.Contains("."))
                {
                    var lastDotIndex = fullClassName.LastIndexOf('.');
                    var namespaceStr = fullClassName.Substring(0, lastDotIndex);
                    simpleClassName = fullClassName.Substring(lastDotIndex + 1);

                    // 将命名空间转换为路径，如 "MyApp.Forms" -> "MyApp\\Forms"
                    namespacePath = namespaceStr.Replace('.', Path.DirectorySeparatorChar);

                    Debug.WriteLine($"命名空间路径: {namespacePath}");
                    Debug.WriteLine($"类名: {simpleClassName}");
                }

                // 获取搜索起点
                var searchRoots = GetSearchRoots();

                foreach (var searchRoot in searchRoots)
                {
                    Debug.WriteLine($"从根目录搜索: {searchRoot}");

                    // 首先向上查找项目根目录(包含 .csproj 的目录)
                    var projectRoot = FindProjectRoot(searchRoot);

                    if (!string.IsNullOrEmpty(projectRoot))
                    {
                        Debug.WriteLine($"找到项目根: {projectRoot}");

                        // 方法A: 使用命名空间路径精确查找
                        var exactPath = TryFindWithNamespacePath(projectRoot, namespacePath, simpleClassName);
                        if (!string.IsNullOrEmpty(exactPath))
                        {
                            Debug.WriteLine($"[精确匹配] 找到项目: {exactPath}");
                            return exactPath;
                        }

                        // 方法B: 在项目根目录下递归搜索类文件
                        var searchPath = SearchClassFileRecursively(projectRoot, simpleClassName);
                        if (!string.IsNullOrEmpty(searchPath))
                        {
                            Debug.WriteLine($"[递归搜索] 找到项目: {searchPath}");
                            return searchPath;
                        }
                    }
                }

                Debug.WriteLine("未找到项目路径");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetProjectPathFromClassName 失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取搜索起点列表
        /// </summary>
        private List<string> GetSearchRoots()
        {
            var roots = new List<string>();

            // 1. 当前工作目录
            if (!string.IsNullOrEmpty(Environment.CurrentDirectory))
            {
                roots.Add(Environment.CurrentDirectory);
            }

            // 2. 应用程序域基目录
            if (!string.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory))
            {
                roots.Add(AppDomain.CurrentDomain.BaseDirectory);
            }

            // 3. 执行程序集位置
            var executingAssembly = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(executingAssembly))
            {
                roots.Add(Path.GetDirectoryName(executingAssembly));
            }

            // 4. VS 环境变量(如果存在)
            var vsSolutionDir = Environment.GetEnvironmentVariable("VS_SOLUTION_DIR");
            if (!string.IsNullOrEmpty(vsSolutionDir) && Directory.Exists(vsSolutionDir))
            {
                roots.Add(vsSolutionDir);
            }

            // 5. 尝试从调用堆栈获取路径
            try
            {
                var stackTrace = new System.Diagnostics.StackTrace(true);
                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    var fileName = frame?.GetFileName();
                    if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    {
                        roots.Add(Path.GetDirectoryName(fileName));
                    }
                }
            }
            catch { }

            // 去重并过滤
            return roots
                .Where(r => !string.IsNullOrEmpty(r) && Directory.Exists(r))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 向上查找项目根目录(包含 .csproj 的目录)
        /// </summary>
        private string FindProjectRoot(string startPath)
        {
            try
            {
                var currentDir = new DirectoryInfo(startPath);

                while (currentDir != null)
                {
                    // 优先查找 .sln 文件
                    if (currentDir.GetFiles("*.sln").Any())
                    {
                        return currentDir.FullName;
                    }

                    // 查找 .csproj 或 .vbproj 文件
                    var projectFiles = currentDir.GetFiles("*.csproj")
                        .ToList();

                    if (projectFiles.Any())
                    {
                        Debug.WriteLine($"找到项目目录: {currentDir.FullName}");
                        return currentDir.FullName;
                    }

                    currentDir = currentDir.Parent;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FindProjectRoot 错误: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// 方法A: 使用命名空间路径精确查找
        /// </summary>
        private string TryFindWithNamespacePath(string projectRoot, string namespacePath, string className)
        {
            try
            {
                if (string.IsNullOrEmpty(namespacePath))
                {
                    return null;
                }

                // 构造可能的文件路径
                var possiblePaths = new[]
                {
                    Path.Combine(projectRoot, namespacePath, $"{className}.cs"),
                    Path.Combine(projectRoot, namespacePath, $"{className}.Designer.cs")
                };

                foreach (var path in possiblePaths)
                {
                    Debug.WriteLine($"检查路径: {path}");

                    if (File.Exists(path))
                    {
                        Debug.WriteLine($"找到文件: {path}");
                        return Path.Combine(projectRoot, namespacePath);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TryFindWithNamespacePath 错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 方法B: 在项目根目录下递归搜索类文件
        /// </summary>
        private string SearchClassFileRecursively(string projectRoot, string className)
        {
            try
            {
                Debug.WriteLine($"在 {projectRoot} 中递归搜索 {className}");

                // 搜索所有可能的文件
                var patterns = new[] { $"{className}.cs", $"{className}.Designer.cs", $"{className}.vb" };

                foreach (var pattern in patterns)
                {
                    var files = Directory.GetFiles(projectRoot, pattern, SearchOption.AllDirectories)
                        .Where(f => !IsExcludedPath(f))  // 排除 bin、obj 等目录
                        .ToList();

                    if (files.Any())
                    {
                        Debug.WriteLine($"找到文件: {files[0]}");

                        // 返回项目根目录
                        return new FileInfo(files[0]).Directory.FullName;
                    }
                }

                // 搜索子目录中的项目
                var subProjects = Directory.GetDirectories(projectRoot)
                    .Where(d => !IsExcludedPath(d))
                    .Where(d => Directory.GetFiles(d, "*.csproj").Any())
                    .ToList();

                foreach (var subProject in subProjects)
                {
                    Debug.WriteLine($"检查子项目: {subProject}");

                    var result = SearchClassFileRecursively(subProject, className);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchClassFileRecursively 错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 判断路径是否应该被排除
        /// </summary>
        private bool IsExcludedPath(string path)
        {
            var excludedNames = new[] { "bin", "obj", ".vs", ".git", "packages", "node_modules" };

            return excludedNames.Any(excluded =>
                path.Contains($"{Path.DirectorySeparatorChar}{excluded}{Path.DirectorySeparatorChar}") ||
                path.EndsWith($"{Path.DirectorySeparatorChar}{excluded}"));
        }


        /// <summary>
        /// 方法2: 通过 Component.Site 获取
        /// </summary>
        private string GetProjectPathFromSite()
        {
            try
            {
                var component = context?.Instance as IComponent;
                if (component?.Site == null)
                {
                    Debug.WriteLine("Component.Site 不可用");
                    return null;
                }

                Debug.WriteLine($"Component: {component.GetType().FullName}");
                Debug.WriteLine($"Site.Name: {component.Site.Name}");

                // 尝试获取设计器加载器
                var loader = designerHost?.GetService(typeof(System.ComponentModel.Design.Serialization.IDesignerSerializationService));

                if (loader != null)
                {
                    Debug.WriteLine("DesignerSerializationService 可用");
                }

                // 通过 IReferenceService 查找引用
                var refService = context.GetService(typeof(IReferenceService)) as IReferenceService;
                if (refService != null)
                {
                    Debug.WriteLine("IReferenceService 可用");
                    // 可以通过 refService 获取组件引用信息
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetProjectPathFromSite 失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 方法3: 搜索文件系统
        /// </summary>
        private string SearchProjectPath()
        {
            try
            {
                var searchRoots = new[]
                {
                    Environment.CurrentDirectory,
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)
                }.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();

                foreach (var root in searchRoots)
                {
                    Debug.WriteLine($"搜索根目录: {root}");

                    var currentDir = new DirectoryInfo(root);
                    while (currentDir != null)
                    {
                        var projectFiles = currentDir.GetFiles("*.csproj")
                            .Concat(currentDir.GetFiles("*.vbproj"))
                            .ToList();

                        if (projectFiles.Count > 0)
                        {
                            Debug.WriteLine($"找到项目: {currentDir.FullName}");
                            return currentDir.FullName;
                        }

                        currentDir = currentDir.Parent;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchProjectPath 失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 检查目录是否包含当前组件
        /// </summary>
        private bool ContainsComponent(string projectPath)
        {
            if (designerHost == null)
            {
                return false;
            }

            var className = designerHost.RootComponentClassName;
            if (string.IsNullOrEmpty(className))
            {
                return false;
            }

            var simpleClassName = className.Contains(".")
                ? className.Substring(className.LastIndexOf('.') + 1)
                : className;

            // 搜索源文件
            return Directory.GetFiles(projectPath, $"{simpleClassName}.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(projectPath, $"{simpleClassName}.Designer.cs", SearchOption.AllDirectories))
                .Any(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"));
        }

        /// <summary>
        /// 获取类的完整信息
        /// </summary>
        public void DebugShowComponentInfo()
        {
            Debug.WriteLine("===== 组件详细信息 =====");

            if (context?.Instance != null)
            {
                var instance = context.Instance;
                Debug.WriteLine($"Instance.GetType(): {instance.GetType().FullName}");
                Debug.WriteLine($"Instance.GetType().Assembly: {instance.GetType().Assembly.FullName}");
                Debug.WriteLine($"Instance.GetType().BaseType: {instance.GetType().BaseType?.FullName}");
            }

            if (designerHost != null)
            {
                Debug.WriteLine($"RootComponent: {designerHost.RootComponent}");
                Debug.WriteLine($"RootComponent.GetType(): {designerHost.RootComponent?.GetType().FullName}");
                Debug.WriteLine($"RootComponentClassName: {designerHost.RootComponentClassName}");

                var rootComp = designerHost.RootComponent;
                if (rootComp != null)
                {
                    // 尝试通过反射获取实际类型
                    var actualType = rootComp.GetType();
                    Debug.WriteLine($"ActualType: {actualType.FullName}");
                    Debug.WriteLine($"ActualType.Assembly: {actualType.Assembly.Location}");

                    if (rootComp is Control ctrl)
                    {
                        Debug.WriteLine($"Control.Name: {ctrl.Name}");
                        Debug.WriteLine($"Control.Text: {ctrl.Text}");
                    }

                    if (rootComp is IComponent comp && comp.Site != null)
                    {
                        Debug.WriteLine($"Site.Name: {comp.Site.Name}");
                        Debug.WriteLine($"Site.Container: {comp.Site.Container}");
                    }
                }
            }

            Debug.WriteLine("========================");
        }
    }
}
