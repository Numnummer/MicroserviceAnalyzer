using System.Collections.Concurrent;

namespace MicroserviceAnalyzer.BL.Models;

/// <summary>
/// Здесь представлена работа с файлами и иерархией директорий.
/// </summary>
public class FileSystem
{
    public class TreeNode
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string OldFullPath { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEditing { get; set; }
        public long Size { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<TreeNode>? Children { get; set; }
        public TreeNode? Parent { get; set; }
        public FileAttributes Attributes { get; set; }
        
        public bool HasChildren => Children?.Count > 0;
        
        public string Icon => IsDirectory ? 
            (IsExpanded ? "📂" : "📁") : 
            GetFileIcon();
        
        private string GetFileIcon()
        {
            var ext = Path.GetExtension(Name).ToLower();
            return ext switch
            {
                ".txt" => "📄",
                ".cs" => "💻",
                ".json" => "📋",
                ".xml" => "📋",
                ".pdf" => "📕",
                ".jpg" or ".jpeg" or ".png" => "🖼️",
                ".zip" or ".rar" => "📦",
                ".exe" => "⚙️",
                ".dll" => "🔧",
                _ => "📃"
            };
        }
    }
    
    private TreeNode _root;
    private readonly ConcurrentDictionary<string, TreeNode> _pathIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, TreeNode> _idIndex = new();
    
    public TreeNode Root => _root;
    
    public event EventHandler<TreeNode>? NodeChanged;
    public event EventHandler<TreeNode>? NodeCreated;
    public event EventHandler<TreeNode>? NodeDeleted;
    
    public FileSystem(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"Директория не найдена: {rootPath}");
        
        _root = BuildTree(rootPath, null);
    }
    
    private TreeNode BuildTree(string path, TreeNode? parent)
    {
        var node = new TreeNode
        {
            Name = Path.GetFileName(path),
            FullPath = path,
            IsDirectory = true,
            Parent = parent
        };
        
        _pathIndex[path] = node;
        _idIndex[node.Id] = node;
        
        var dirInfo = new DirectoryInfo(path);
        node.CreatedDate = dirInfo.CreationTime;
        node.ModifiedDate = dirInfo.LastWriteTime;
        node.Attributes = dirInfo.Attributes;
        
        node.Children = new List<TreeNode>();
        
        try
        {
            // Добавляем поддиректории
            foreach (var dir in Directory.GetDirectories(path))
            {
                if (Path.GetFileName(dir) == "bin" || Path.GetFileName(dir) == "obj")
                    continue;
                var dirNode = BuildTree(dir, node);
                node.Children.Add(dirNode);
            }
            
            // Добавляем файлы
            foreach (var file in Directory.GetFiles(path))
            {
                var fileNode = CreateFileNode(file, node);
                node.Children.Add(fileNode);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Пропускаем директории без доступа
        }
        
        return node;
    }
    
    private TreeNode CreateFileNode(string fileName, TreeNode parent)
    {
        var node = new TreeNode
        {
            Name = Path.GetFileName(fileName),
            FullPath = Path.Combine(parent.FullPath, Path.GetFileName(fileName)),
            IsDirectory = false,
            Parent = parent
        };
        
        _idIndex[node.Id] = node;
        
        return node;
    }
    
    public bool RenameNode(TreeNode node, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return false;
        
        try
        {
            node.OldFullPath = node.FullPath;
            var fullPathParts = node.FullPath.Split(Path.DirectorySeparatorChar);            
            fullPathParts[^1] = newName;
            node.FullPath = Path.DirectorySeparatorChar + Path.Combine(fullPathParts);
            NodeChanged?.Invoke(this, node);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка переименования: {ex.Message}");
            return false;
        }
    }
    
    public TreeNode CreateDirectory(TreeNode parent, string directoryName)
    {
        try
        {
            var newNode = new TreeNode
            {
                Name = directoryName,
                FullPath = Path.Combine(parent.FullPath, directoryName),
                IsDirectory = true,
                Parent = parent,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
            
            parent.Children ??= new List<TreeNode>();
            parent.Children.Add(newNode);
            parent.IsExpanded = true; 
            
            _idIndex[newNode.Id] = newNode;
            
            NodeCreated?.Invoke(this, newNode);
            return newNode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка создания директории: {ex.Message}");
            throw;
        }
    }
    
    public TreeNode CreateFile(TreeNode parent, string fileName)
    {
        try
        {
            var newNode = CreateFileNode(fileName, parent);
            parent.Children ??= new List<TreeNode>();
            parent.Children.Add(newNode);
            parent.IsExpanded = true;
            
            NodeCreated?.Invoke(this, newNode);
            return newNode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка создания файла: {ex.Message}");
            throw;
        }
    }
    
    public bool DeleteNode(TreeNode node)
    {
        try
        {
            // Удаляем из родительской коллекции
            node.Parent?.Children?.Remove(node);
            
            // Удаляем из индексов
            _idIndex.TryRemove(node.Id, out _);
            
            // Рекурсивно удаляем детей из индексов
            RemoveFromIndexes(node);
            
            NodeDeleted?.Invoke(this, node);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка удаления: {ex.Message}");
            return false;
        }
    }
    
    private void RemoveFromIndexes(TreeNode node)
    {
        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                _idIndex.TryRemove(child.Id, out _);
                RemoveFromIndexes(child);
            }
        }
    }
    
    public TreeNode? GetNodeByPath(string path) => 
        _pathIndex.GetValueOrDefault(path);
    
    public TreeNode? GetNodeById(Guid id) => 
        _idIndex.GetValueOrDefault(id);
    
    public IEnumerable<TreeNode> TraverseDfs(bool preOrder = true)
    {
        return TraverseDfs(_root, preOrder);
    }
    
    public IEnumerable<TreeNode> TraverseDfs(TreeNode node, bool preOrder)
    {
        if (preOrder) yield return node;
        
        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                foreach (var descendant in TraverseDfs(child, preOrder))
                {
                    yield return descendant;
                }
            }
        }
        
        if (!preOrder) yield return node;
    }
}