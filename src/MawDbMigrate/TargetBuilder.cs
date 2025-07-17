using MawDbMigrate.Models.Extensions;
using MawDbMigrate.Models.Source;

namespace MawDbMigrate;

public class TargetBuilder
{
    readonly Models.Target.Db _target = new();
    readonly Dictionary<short, Guid> _userIdMap = [];
    readonly Dictionary<short, Guid> _roleIdMap = [];
    readonly Dictionary<short, Guid> _photoCategoryIdMap = [];
    readonly Dictionary<int, Models.Target.Media> _photoIdMap = [];
    readonly Dictionary<short, Guid> _videoCategoryIdMap = [];
    readonly Dictionary<int, Models.Target.Media> _videoIdMap = [];
    readonly Dictionary<string, Models.Target.Location> _locationMap = [];

    Models.Target.User? _admin = null;

    public Models.Target.User Admin
    {
        get
        {
            _admin ??= _target.Users
                .Single(x => string.Equals(x.Email, "mmorano@mikeandwan.us", StringComparison.OrdinalIgnoreCase));

            if (_admin == null)
            {
                throw new Exception("Admin user not found!");
            }

            return _admin;
        }
    }

    public Models.Target.Db Build(Db src)
    {
        BuildLocationLookup(src.Photos, src.PhotoGpsOverrides, src.Videos, src.VideoGpsOverrides);

        PrepareUsers(src.Users);
        PrepareRoles(src.Roles);
        PrepareUserRoles(src.UserRoles);

        PrepareCategories(src.PhotoCategories, src.VideoCategories);
        PrepareCategoryRoles(src.PhotoCategoryRoles, src.VideoCategoryRoles);
        PrepareMedia(src.Photos, src.Videos);
        PrepareMediaFiles(src.Photos, src.Videos);
        PrepareCategoryMedia(src.Photos, src.PhotoCategories, src.Videos, src.VideoCategories);
        PrepareComments(src.PhotoComments, src.VideoComments);
        PrepareFavorites(src.PhotoRatings, src.VideoRatings);
        PrepareLocations(
            src.PhotoReverseGeocodes,
            src.Photos,
            src.PhotoGpsOverrides,
            src.VideoReverseGeocodes,
            src.Videos,
            src.VideoGpsOverrides
        );
        AssignLocations(src.Photos, src.PhotoGpsOverrides, src.Videos, src.VideoGpsOverrides);
        PreparePointsOfInterest(src.PhotoPointOfInterests, src.VideoPointOfInterests);

        return _target;
    }

    void PrepareUsers(IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            if(string.IsNullOrEmpty(user.Email))
            {
                user.Email = $"{user.Username}@example.com";
            }

            var targetUser = user.ToTarget();
            _userIdMap.Add(user.Id, targetUser.Id);
            _target.Users.Add(targetUser);
        }
    }

    void PrepareRoles(IEnumerable<Role> roles)
    {
        foreach (var role in roles)
        {
            var targetRole = role.ToTarget(Admin.Id);
            _roleIdMap.Add(role.Id, targetRole.Id);
            _target.Roles.Add(targetRole);
        }
    }

    void PrepareUserRoles(IEnumerable<UserRole> userRoles)
    {
        foreach (var userRole in userRoles)
        {
            var targetUserRole = new Models.Target.UserRole
            {
                UserId = _userIdMap[userRole.UserId],
                RoleId = _roleIdMap[userRole.RoleId],
                Created = DateTime.MinValue,
                CreatedBy = Admin.Id
            };
            _target.UserRoles.Add(targetUserRole);
        }
    }

    void PrepareCategories(
        IEnumerable<PhotoCategory> photoCategories,
        IEnumerable<VideoCategory> videoCategories
    )
    {
        var pc = photoCategories
            .OrderBy(x => x.EffectiveDate)
            .ToList();

        var vc = videoCategories
            .OrderBy(x => x.EffectiveDate)
            .ToList();

        var pcIdx = 0;
        var vcIdx = 0;

        // merge these chronologically and artificially adjust effective date if needed to try to retain current sort order
        while (pcIdx < pc.Count || vcIdx < vc.Count)
        {
            if (pcIdx >= pc.Count || (vcIdx < vc.Count && vc[vcIdx].EffectiveDate < pc[pcIdx].EffectiveDate))
            {
                var category = vc[vcIdx];
                var targetCategory = category.ToTarget(Admin.Id);

                _videoCategoryIdMap.Add(category.Id, targetCategory.Id);
                _target.Categories.Add(targetCategory);

                vcIdx++;
            }
            else
            {
                var category = pc[pcIdx];
                var targetCategory = category.ToTarget(Admin.Id);

                _photoCategoryIdMap.Add(category.Id, targetCategory.Id);
                _target.Categories.Add(targetCategory);

                pcIdx++;
            }
        }

        if (pc.Count + vc.Count != _target.Categories.Count)
        {
            throw new Exception("Category count mismatch!");
        }
    }

    void PrepareCategoryRoles(
        IEnumerable<PhotoCategoryRole> photoCategoryRoles,
        IEnumerable<VideoCategoryRole> videoCategoryRoles
    )
    {
        foreach (var cr in photoCategoryRoles)
        {
            var targetCategoryRole = new Models.Target.CategoryRole
            {
                CategoryId = _photoCategoryIdMap[cr.CategoryId],
                RoleId = _roleIdMap[cr.RoleId],
                Created = DateTime.MinValue,
                CreatedBy = Admin.Id
            };

            _target.CategoryRoles.Add(targetCategoryRole);
        }

        foreach (var cr in videoCategoryRoles)
        {
            var targetCategoryRole = new Models.Target.CategoryRole
            {
                CategoryId = _videoCategoryIdMap[cr.CategoryId],
                RoleId = _roleIdMap[cr.RoleId],
                Created = DateTime.MinValue,
                CreatedBy = Admin.Id
            };

            _target.CategoryRoles.Add(targetCategoryRole);
        }
    }

    void PrepareMedia(
        IEnumerable<Photo> photos,
        IEnumerable<Video> videos
    )
    {
        // set media chronologically based on category order
        foreach (var category in _target.Categories)
        {
            if (category.LegacyType == 'p')
            {
                _target.Media.AddRange(
                    photos
                        .Where(x => x.CategoryId == category.LegacyId)
                        .OrderBy(x => x.Id)
                        .Select(x =>
                        {
                            var media = x.ToTarget(Admin.Id);

                            _photoIdMap.Add(x.Id, media);

                            return media;
                        })
                );
            }
            else
            {
                _target.Media.AddRange(
                    videos
                        .Where(x => x.CategoryId == category.LegacyId)
                        .OrderBy(x => x.Id)
                        .Select(x =>
                        {
                            var media = x.ToTarget(Admin.Id);

                            _videoIdMap.Add(x.Id, media);

                            return media;
                        })
                );
            }
        }
    }

    void PrepareComments(
        IEnumerable<PhotoComment> photoComments,
        IEnumerable<VideoComment> videoComments
    )
    {
        foreach (var comment in photoComments)
        {
            // we may have thrown away the google demo photo instances - so ignore if missing
            if (_photoIdMap.TryGetValue(comment.PhotoId, out var media))
            {
                var targetComment = new Models.Target.Comment
                {
                    Id = Guid.CreateVersion7(),
                    MediaId = _photoIdMap[media.LegacyId].Id,
                    Created = comment.EntryDate,
                    CreatedBy = _userIdMap[comment.UserId],
                    Modified = DateTime.MinValue,
                    Body = comment.Message
                };

                _target.Comments.Add(targetComment);
            }
        }

        foreach (var comment in videoComments)
        {
            var targetComment = new Models.Target.Comment
            {
                Id = Guid.CreateVersion7(),
                MediaId = _videoIdMap[comment.VideoId].Id,
                Created = comment.EntryDate,
                CreatedBy = _userIdMap[comment.UserId],
                Modified = DateTime.MinValue,
                Body = comment.Message
            };

            _target.Comments.Add(targetComment);
        }
    }

    void PrepareFavorites(
        IEnumerable<PhotoRating> photoRatings,
        IEnumerable<VideoRating> videoRatings
    )
    {
        foreach (var rating in photoRatings)
        {
            // we may have thrown away the google demo photo instances - so ignore if missing
            if (_photoIdMap.TryGetValue(rating.PhotoId, out var media))
            {
                var targetFavorite = new Models.Target.Favorite
                {
                    MediaId = _photoIdMap[media.LegacyId].Id,
                    CreatedBy = _userIdMap[rating.UserId],
                    Created = DateTime.MinValue
                };

                _target.Favorites.Add(targetFavorite);
            }
        }

        foreach (var rating in videoRatings)
        {
            var targetFavorite = new Models.Target.Favorite
            {
                MediaId = _videoIdMap[rating.VideoId].Id,
                CreatedBy = _userIdMap[rating.UserId],
                Created = DateTime.MinValue
            };

            _target.Favorites.Add(targetFavorite);
        }
    }

    string BuildLocationKey(
        decimal? latitude,
        decimal? longitude
    )
    {
        if (latitude == null || longitude == null)
        {
            return string.Empty;
        }

        return $"{latitude:f6},{longitude:f6}";
    }

    bool ShouldAddLocationKeyLookup(string key) => !(string.IsNullOrEmpty(key) || _locationMap.ContainsKey(key));
    bool IsValidLocationKey(string key) => _locationMap.ContainsKey(key);

    Models.Target.Location CreateLocation(decimal latitude, decimal longitude)
    {
        var location = new Models.Target.Location
        {
            Id = Guid.CreateVersion7(),
            Latitude = latitude,
            Longitude = longitude
        };

        _target.Locations.Add(location);

        return location;
    }

    void BuildLocationLookup(
        IEnumerable<Photo> photos,
        IEnumerable<PhotoGpsOverride> photoGpsOverrides,
        IEnumerable<Video> videos,
        IEnumerable<VideoGpsOverride> videoGpsOverrides
    )
    {
        foreach (var photo in photos)
        {
            var key = BuildLocationKey(photo.GpsLatitude, photo.GpsLongitude);

            if (ShouldAddLocationKeyLookup(key))
            {
                _locationMap.Add(key, CreateLocation(photo.GpsLatitude!.Value, photo.GpsLongitude!.Value));
            }
        }

        foreach (var photo in photoGpsOverrides)
        {
            var key = BuildLocationKey(photo.Latitude, photo.Longitude);

            if (ShouldAddLocationKeyLookup(key))
            {
                _locationMap.Add(key, CreateLocation(photo.Latitude, photo.Longitude));
            }
        }

        foreach (var video in videos)
        {
            var key = BuildLocationKey(video.GpsLatitude, video.GpsLongitude);

            if (ShouldAddLocationKeyLookup(key))
            {
                _locationMap.Add(key, CreateLocation(video.GpsLatitude!.Value, video.GpsLongitude!.Value));
            }
        }

        foreach (var video in videoGpsOverrides)
        {
            var key = BuildLocationKey(video.Latitude, video.Longitude);

            if (ShouldAddLocationKeyLookup(key))
            {
                _locationMap.Add(key, CreateLocation(video.Latitude, video.Longitude));
            }
        }
    }

    void PrepareLocations(
        IEnumerable<PhotoReverseGeocode> photoReverseGeocodes,
        IEnumerable<Photo> photos,
        IEnumerable<PhotoGpsOverride> photoGpsOverrides,
        IEnumerable<VideoReverseGeocode> videoReverseGeocodes,
        IEnumerable<Video> videos,
        IEnumerable<VideoGpsOverride> videoGpsOverrides
    )
    {
        foreach (var photo in photos)
        {
            var key = BuildLocationKey(photo.GpsLatitude, photo.GpsLongitude);

            if (IsValidLocationKey(key))
            {
                var targetMedia = _photoIdMap[photo.Id];

                targetMedia.LocationId = _locationMap[key].Id;
            }
        }

        foreach (var video in videos)
        {
            var key = BuildLocationKey(video.GpsLatitude, video.GpsLongitude);

            if (IsValidLocationKey(key))
            {
                var targetMedia = _videoIdMap[video.Id];

                targetMedia.LocationId = _locationMap[key].Id;
            }
        }

        foreach (var photo in photoGpsOverrides)
        {
            var key = BuildLocationKey(photo.Latitude, photo.Longitude);

            if (IsValidLocationKey(key))
            {
                var targetMedia = _photoIdMap[photo.PhotoId];

                targetMedia.LocationId = _locationMap[key].Id;
            }
        }

        foreach (var video in videoGpsOverrides)
        {
            var key = BuildLocationKey(video.Latitude, video.Longitude);

            if (IsValidLocationKey(key))
            {
                var targetMedia = _videoIdMap[video.VideoId];

                targetMedia.LocationId = _locationMap[key].Id;
            }
        }

        foreach (var reverseGeocode in photoReverseGeocodes)
        {
            var targetMedia = _photoIdMap[reverseGeocode.PhotoId];

            if (reverseGeocode.IsOverride)
            {
                var gpsOverride = photoGpsOverrides
                    .Single(x => x.PhotoId == reverseGeocode.PhotoId);
                var locationKey = BuildLocationKey(gpsOverride.Latitude, gpsOverride.Longitude);
                var location = _locationMap[locationKey];

                reverseGeocode.Populate(location);
                targetMedia.LocationOverrideId = location.Id;
            }
            else
            {
                var photo = photos
                    .Single(x => x.Id == reverseGeocode.PhotoId);
                var locationKey = BuildLocationKey(photo.GpsLatitude, photo.GpsLongitude);
                var location = _locationMap[locationKey];

                reverseGeocode.Populate(location);
                targetMedia.LocationId = location.Id;
            }
        }

        foreach (var reverseGeocode in videoReverseGeocodes)
        {
            var targetMedia = _videoIdMap[reverseGeocode.VideoId];

            if (reverseGeocode.IsOverride)
            {
                var gpsOverride = videoGpsOverrides
                    .Single(x => x.VideoId == reverseGeocode.VideoId);
                var locationKey = BuildLocationKey(gpsOverride.Latitude, gpsOverride.Longitude);
                var location = _locationMap[locationKey];

                reverseGeocode.Populate(location);
                targetMedia.LocationOverrideId = location.Id;
            }
            else
            {
                var video = videos
                    .Single(x => x.Id == reverseGeocode.VideoId);
                var locationKey = BuildLocationKey(video.GpsLatitude, video.GpsLongitude);
                var location = _locationMap[locationKey];

                reverseGeocode.Populate(location);
                targetMedia.LocationId = location.Id;
            }
        }
    }

    void PreparePointsOfInterest(
        IEnumerable<PhotoPointOfInterest> photoPointsOfInterest,
        IEnumerable<VideoPointOfInterest> videoPointsOfInterest
    )
    {
        var list = new List<Models.Target.PointOfInterest>();

        foreach (var poi in photoPointsOfInterest)
        {
            var targetMedia = _photoIdMap[poi.PhotoId];

            if (poi.IsOverride)
            {
                var locationId = targetMedia.LocationOverrideId;

                if (locationId == null)
                {
                    Console.WriteLine($"Photo {poi.PhotoId} has a POI override but no location override. skipping.");
                    continue;
                }

                list.Add(new Models.Target.PointOfInterest
                {
                    LocationId = (Guid)locationId!,
                    Type = poi.PoiType,
                    Name = poi.PoiName
                });
            }
            else
            {
                var locationId = targetMedia.LocationId;

                if (locationId == null)
                {
                    Console.WriteLine($"Photo {poi.PhotoId} has a POI but no location. skipping.");
                    continue;
                }

                list.Add(new Models.Target.PointOfInterest
                {
                    LocationId = (Guid)locationId!,
                    Type = poi.PoiType,
                    Name = poi.PoiName
                });
            }
        }

        foreach (var poi in videoPointsOfInterest)
        {
            var targetMedia = _videoIdMap[poi.VideoId];

            if (poi.IsOverride)
            {
                var locationId = targetMedia.LocationOverrideId;

                if (locationId == null)
                {
                    Console.WriteLine($"Video {poi.VideoId} has a POI override but no location override. skipping.");
                    continue;
                }

                list.Add(new Models.Target.PointOfInterest
                {
                    LocationId = (Guid)locationId!,
                    Type = poi.PoiType,
                    Name = poi.PoiName
                });
            }
            else
            {
                var locationId = targetMedia.LocationId;

                if (locationId == null)
                {
                    Console.WriteLine($"Video {poi.VideoId} has a POI but no location. skipping.");
                    continue;
                }

                list.Add(new Models.Target.PointOfInterest
                {
                    LocationId = (Guid)locationId!,
                    Type = poi.PoiType,
                    Name = poi.PoiName
                });
            }
        }

        _target.PointsOfInterest.AddRange(list
            .DistinctBy(x => new { x.LocationId, x.Type, x.Name })
            .ToList()
        );
    }

    void AssignLocations(
        IEnumerable<Photo> photos,
        IEnumerable<PhotoGpsOverride> photoGpsOverrides,
        IEnumerable<Video> videos,
        IEnumerable<VideoGpsOverride> videoGpsOverrides
    )
    {
        var gpsPhotos = photos
            .Where(x => x.GpsLatitude != null && x.GpsLongitude != null);

        foreach (var photo in gpsPhotos)
        {
            var locationKey = BuildLocationKey(photo.GpsLatitude, photo.GpsLongitude);
            var media = _photoIdMap[photo.Id];

            media.LocationId = string.IsNullOrWhiteSpace(locationKey) ? null : _locationMap[locationKey].Id;
        }

        foreach (var gpsOverride in photoGpsOverrides)
        {
            var locationKey = BuildLocationKey(gpsOverride.Latitude, gpsOverride.Longitude);
            var media = _photoIdMap[gpsOverride.PhotoId];

            media.LocationOverrideId = string.IsNullOrWhiteSpace(locationKey) ? null : _locationMap[locationKey].Id;
        }

        var gpsVideos = videos
            .Where(x => x.GpsLatitude != null && x.GpsLongitude != null);

        foreach (var video in gpsVideos)
        {
            var locationKey = BuildLocationKey(video.GpsLatitude, video.GpsLongitude);
            var media = _videoIdMap[video.Id];

            media.LocationId = string.IsNullOrWhiteSpace(locationKey) ? null : _locationMap[locationKey].Id;
        }

        foreach (var gpsOverride in videoGpsOverrides)
        {
            var locationKey = BuildLocationKey(gpsOverride.Latitude, gpsOverride.Longitude);
            var media = _videoIdMap[gpsOverride.VideoId];

            media.LocationOverrideId = string.IsNullOrWhiteSpace(locationKey) ? null : _locationMap[locationKey].Id;
        }
    }

    // copied from MawMediaMigrate.StringExtensions
    static string FixupMediaDirectory(string path) => path
        .Replace(" ", "-")
        .Replace("_", "-");

    void PrepareMediaFiles(
        IEnumerable<Photo> photos,
        IEnumerable<Video> videos
    )
    {
        foreach (var photo in photos)
        {
            var media = _photoIdMap[photo.Id];

            var srcPath = Path.Combine(
                FixupMediaDirectory(Path.GetDirectoryName(photo.SrcPath!)!),
                Path.GetFileName(photo.SrcPath!)
            )
            .Replace("/images/", "/assets/");

            var targetMediaFile = new Models.Target.MediaFile
            {
                Id = Guid.CreateVersion7(),
                MediaId = media.Id,
                MediaTypeId = Models.Target.MediaType.Photo.Id,
                ScaleId = Models.Target.Scale.Src.Id,
                Width = photo.SrcWidth,
                Height = photo.SrcHeight,
                Bytes = photo.SrcSize ?? 0,
                Path = srcPath
            };

            _target.MediaFiles.Add(targetMediaFile);
        }

        foreach (var video in videos)
        {
            var media = _videoIdMap[video.Id];

            var srcPath = Path.Combine(
                FixupMediaDirectory(Path.GetDirectoryName(video.RawPath!)!),
                Path.GetFileName(video.RawPath!)
            )
            .Replace("/raw/", "/src/")
            .Replace("/movies/", "/assets/");

            var targetMediaFile = new Models.Target.MediaFile
            {
                Id = Guid.CreateVersion7(),
                MediaId = media.Id,
                MediaTypeId = Models.Target.MediaType.Video.Id,
                ScaleId = Models.Target.Scale.Src.Id,
                Width = video.RawWidth,
                Height = video.RawHeight,
                Bytes = video.RawSize,
                Path = srcPath
            };

            _target.MediaFiles.Add(targetMediaFile);
        }
    }

    void PrepareCategoryMedia(
        IEnumerable<Photo> photos,
        IEnumerable<PhotoCategory> photoCategories,
        IEnumerable<Video> videos,
        IEnumerable<VideoCategory> videoCategories
    ) {
        foreach (var photo in photos)
        {
            var category = photoCategories.Single(x => x.Id == photo.CategoryId);
            var media = _photoIdMap[photo.Id];

            var targetCategoryMedia = new Models.Target.CategoryMedia
            {
                CategoryId = _photoCategoryIdMap[category.Id],
                MediaId = media.Id,
                IsTeaser = string.Equals(category.TeaserPhotoSqPath, photo.XsSqPath, StringComparison.OrdinalIgnoreCase),
                Created = DateTime.MinValue,
                CreatedBy = Admin.Id,
                Modified = DateTime.MinValue,
                ModifiedBy = Admin.Id
            };

            _target.CategoryMedia.Add(targetCategoryMedia);
        }

        foreach (var video in videos)
        {
            var category = videoCategories.Single(x => x.Id == video.CategoryId);
            var media = _videoIdMap[video.Id];

            var targetCategoryMedia = new Models.Target.CategoryMedia
            {
                CategoryId = _videoCategoryIdMap[category.Id],
                MediaId = media.Id,
                IsTeaser = string.Equals(category.TeaserImageSqPath, video.ThumbSqPath, StringComparison.OrdinalIgnoreCase),
                Created = DateTime.MinValue,
                CreatedBy = Admin.Id,
                Modified = DateTime.MinValue,
                ModifiedBy = Admin.Id
            };

            _target.CategoryMedia.Add(targetCategoryMedia);
        }
    }
}
