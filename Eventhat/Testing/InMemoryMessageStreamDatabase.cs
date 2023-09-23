using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.Helpers;

namespace Eventhat.Testing;

public class InMemoryMessageStreamDatabase : IMessageStreamDatabase
{
    private readonly List<AdminStream> _adminStreams = new();
    private readonly List<AdminUser> _adminUsers = new();
    private readonly List<CreatorVideo> _creatorVideos = new();
    private readonly List<MessageEntity> _messagesTable = new();
    private readonly List<UserCredentials> _userCredentials = new();
    private readonly List<VideoOperation> _videoOperations = new();

    private List<Page> _pages = new();

    public IQueryable<VideoOperation> VideoOperations
    {
        get
        {
            lock (_videoOperations)
            {
                return _videoOperations.AsQueryable();
            }
        }
    }

    public IQueryable<CreatorVideo> CreatorVideos
    {
        get
        {
            lock (_creatorVideos)
            {
                return _creatorVideos.AsQueryable();
            }
        }
    }

    public IQueryable<AdminUser> AdminUsers
    {
        get
        {
            lock (_adminUsers)
            {
                return _adminUsers.AsQueryable();
            }
        }
    }

    public IQueryable<MessageEntity> Messages
    {
        get
        {
            lock (_messagesTable)
            {
                return _messagesTable.AsQueryable();
            }
        }
    }

    public IQueryable<AdminStream> AdminStreams
    {
        get
        {
            lock (_adminStreams)
            {
                return _adminStreams.AsQueryable();
            }
        }
    }

    public Task WriteMessageAsync(Guid id, string streamName, string type, string metadata, string data, int? expectedVersion)
    {
        lock (_messagesTable)
        {
            var streamVersion = _messagesTable.Count(m => m.StreamName == streamName);
            if (expectedVersion != null && streamVersion != expectedVersion)
                throw new Exception($"Version Conflict: stream {streamName}, streamVersion {streamVersion}, expectedVersion {expectedVersion} ");

            _messagesTable.Add(new MessageEntity(id, streamName, type, streamVersion + 1, _messagesTable.Count + 1, data, metadata, DateTimeOffset.Now));

            return Task.CompletedTask;
        }
    }

    public Task<IEnumerable<MessageEntity>> GetCategoryMessages(string categoryName, int fromPosition = 0, int batchSize = 1000)
    {
        return Task.FromResult(_messagesTable.Where(m => m.StreamName.GetCategory() == categoryName && m.GlobalPosition >= fromPosition).OrderBy(m => m.GlobalPosition).Take(batchSize));
    }

    public Task<MessageEntity?> GetLastStreamMessage(string streamName)
    {
        return Task.FromResult(_messagesTable.Where(m => m.StreamName == streamName).MaxBy(m => m.Position));
    }

    public Task<IEnumerable<MessageEntity>> GetStreamMessages(string streamName, int fromPosition = 0, int batchSize = 1000)
    {
        return Task.FromResult(_messagesTable.Where(m => m.StreamName == streamName && m.Position >= fromPosition).OrderBy(m => m.Position).Take(batchSize));
    }

    public IQueryable<Page> Pages
    {
        get
        {
            lock (_pages)
            {
                return _pages.AsQueryable();
            }
        }
    }

    public IQueryable<UserCredentials> UserCredentials
    {
        get
        {
            lock (_userCredentials)
            {
                return _userCredentials.AsQueryable();
            }
        }
    }

    public Task InsertPageAsync(string name, string data)
    {
        lock (_pages)
        {
            if (_pages.Any(p => p.Name == name)) return Task.CompletedTask;

            _pages.Add(new Page(name, data));
            return Task.CompletedTask;
        }
    }

    public Task InsertUserCredentialAsync(Guid id, string email, string passwordHash)
    {
        lock (_userCredentials)
        {
            if (_userCredentials.Any(c => c.Id == id)) return Task.CompletedTask;

            _userCredentials.Add(new UserCredentials(id, email, passwordHash));
            return Task.CompletedTask;
        }
    }

    public Task InsertVideoOperationAsync(Guid traceId, Guid videoId, bool succeeded, string reason)
    {
        lock (_videoOperations)
        {
            _videoOperations.Add(new VideoOperation(traceId, videoId, succeeded, reason));
            return Task.CompletedTask;
        }
    }

    public Task InsertCreatorsVideoAsync(Guid videoId, Uri transcodedUri, int position)
    {
        lock (_creatorVideos)
        {
            if (_creatorVideos.Any(v => v.VideoId == videoId)) return Task.CompletedTask;
            _creatorVideos.Add(new CreatorVideo(videoId, transcodedUri, "Untitled", position));
            return Task.CompletedTask;
        }
    }

    public Task UpdateCreatorsVideoAsync(Guid videoId, string name, int position)
    {
        lock (_creatorVideos)
        {
            var existingVideo = _creatorVideos.Single(v => v.VideoId == videoId && v.Position < position);
            _creatorVideos.Remove(existingVideo);
            _creatorVideos.Add(new CreatorVideo(existingVideo.VideoId, existingVideo.TranscodedUri, name, position));
            return Task.CompletedTask;
        }
    }

    public Task InsertAdminUsersAsync(Guid id)
    {
        lock (_adminUsers)
        {
            if (_adminUsers.Any(u => u.Id == id)) return Task.CompletedTask;
            _adminUsers.Add(new AdminUser(id, string.Empty, false, 0, 0, 0));
            return Task.CompletedTask;
        }
    }

    public Task SetAdminUserEmail(Guid id, string email, int globalPosition)
    {
        lock (_adminUsers)
        {
            var existingEntity = _adminUsers.SingleOrDefault(u => u.Id == id);
            if (existingEntity == null) throw new Exception($"Unknown user with id {id}");

            if (existingEntity.LastIdentityEventGlobalPosition >= globalPosition)
                return Task.CompletedTask;

            _adminUsers.Remove(existingEntity);

            _adminUsers.Add(
                new AdminUser(
                    id,
                    email,
                    existingEntity.RegistrationEmailSent,
                    globalPosition,
                    existingEntity.LoginCount,
                    existingEntity.LastAuthenticationEventGlobalPosition));
            return Task.CompletedTask;
        }
    }

    public Task MarkRegistrationEmailSent(Guid id, int globalPosition)
    {
        lock (_adminUsers)
        {
            var existingEntity = _adminUsers.SingleOrDefault(u => u.Id == id);
            if (existingEntity == null) throw new Exception($"Unknown user with id {id}");

            if (existingEntity.LastIdentityEventGlobalPosition >= globalPosition)
                return Task.CompletedTask;

            _adminUsers.Remove(existingEntity);

            _adminUsers.Add(
                new AdminUser(
                    id,
                    existingEntity.Email,
                    true,
                    globalPosition,
                    existingEntity.LoginCount,
                    existingEntity.LastAuthenticationEventGlobalPosition));
            return Task.CompletedTask;
        }
    }

    public Task IncreaseAdminUserLoginCount(Guid id, int globalPosition)
    {
        lock (_adminUsers)
        {
            var existingEntity = _adminUsers.SingleOrDefault(u => u.Id == id);
            if (existingEntity == null) throw new Exception($"Unknown user with id {id}");

            if (existingEntity.LastAuthenticationEventGlobalPosition >= globalPosition)
                return Task.CompletedTask;

            _adminUsers.Remove(existingEntity);

            _adminUsers.Add(
                new AdminUser(
                    id,
                    existingEntity.Email,
                    existingEntity.RegistrationEmailSent,
                    existingEntity.LastIdentityEventGlobalPosition,
                    existingEntity.LoginCount + 1,
                    globalPosition));
            return Task.CompletedTask;
        }
    }

    public Task UpsertStream(string streamName, Guid messageId, int globalPosition)
    {
        lock (_adminStreams)
        {
            var existingStream = _adminStreams.SingleOrDefault(s => s.StreamName == streamName);
            if (existingStream == null)
            {
                _adminStreams.Add(new AdminStream(streamName, 1, messageId, globalPosition));
            }
            else if (existingStream.LastMessageGlobalPosition < globalPosition)
            {
                _adminStreams.Remove(existingStream);
                _adminStreams.Add(new AdminStream(streamName, existingStream.MessageCount + 1, messageId, globalPosition));
            }

            return Task.CompletedTask;
        }
    }

    public Task UpdatePageAsync(string name, string data)
    {
        lock (_pages)
        {
            _pages = _pages.Where(x => x.Name != name).ToList();
            _pages.Add(new Page(name, data));
            return Task.CompletedTask;
        }
    }
}