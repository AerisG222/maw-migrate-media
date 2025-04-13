using MawDbMigrateTool.Models.Extensions;
using MawDbMigrateTool.Models.Source;

namespace MawDbMigrateTool;

public class TargetBuilder
{
    readonly Models.Target.Db _target = new();
    readonly Dictionary<short, Guid> _userIdMap = [];
    readonly Dictionary<short, Guid> _roleIdMap = [];
    readonly Dictionary<short, Guid> _photoCategoryIdMap = [];
    readonly Dictionary<int, Models.Target.Media> _photoIdMap = [];
    readonly Dictionary<short, Guid> _videoCategoryIdMap = [];
    readonly Dictionary<int, Models.Target.Media> _videoIdMap = [];

    Models.Target.User Admin =>
        _target.Users.Single(x => string.Equals(x.Email, "mmorano@mikeandwan.us", StringComparison.OrdinalIgnoreCase));

    public Models.Target.Db Build(Db src)
    {
        PrepareUsers(src.Users);
        PrepareRoles(src.Roles);
        PrepareUserRoles(src.UserRoles);

        PrepareCategories(src.PhotoCategories, src.VideoCategories);
        PrepareCategoryRoles(src.PhotoCategoryRoles, src.VideoCategoryRoles);
        PrepareMedia(src.Photos, src.Videos);
        PrepareComments(src.PhotoComments, src.VideoComments);
        PrepareRatings(src.PhotoRatings, src.VideoRatings);
        PrepareLocations(
            src.PhotoReverseGeocodes,
            src.Photos,
            src.PhotoGpsOverrides,
            src.VideoReverseGeocodes,
            src.Videos,
            src.VideoGpsOverrides
        );
        PreparePointsOfInterest(src.PhotoPointOfInterests, src.VideoPointOfInterests);

        return _target;
    }

    void PrepareUsers(IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            var targetUser = user.ToTarget();
            _userIdMap.Add(user.Id, targetUser.Id);
            _target.Users.Add(targetUser);
        }
    }

    void PrepareRoles(IEnumerable<Role> roles)
    {
        foreach (var role in roles)
        {
            var targetRole = role.ToTarget();
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
                RoleId = _roleIdMap[userRole.RoleId]
            };
            _target.UserRoles.Add(targetUserRole);
        }
    }

    void PrepareCategories(
        IEnumerable<PhotoCategory> photoCategories,
        IEnumerable<VideoCategory> videoCategories
    ) {
        var pc = photoCategories
            .OrderBy(x => x.SortKey)
            .ToList();

        var vc = videoCategories
            .OrderBy(x => x.SortKey)
            .ToList();

        var pcIdx = 0;
        var vcIdx = 0;

        // merge these chronologically
        while (pcIdx < pc.Count || vcIdx < vc.Count)
        {
            if(pcIdx >= pc.Count || (vcIdx < vc.Count && vc[vcIdx].SortKey < pc[pcIdx].SortKey))
            {
                var category = vc[vcIdx];
                var targetCategory = category.ToTarget(Admin.Id);

                _videoCategoryIdMap.Add(category.Id, targetCategory.Id);
                _target.Categories.Add(targetCategory);

                vcIdx++;
            } else {
                var category = pc[pcIdx];
                var targetCategory = category.ToTarget(Admin.Id);

                _photoCategoryIdMap.Add(category.Id, targetCategory.Id);
                _target.Categories.Add(targetCategory);

                pcIdx++;
            }
        }

        if(pc.Count + vc.Count != _target.Categories.Count)
        {
            throw new Exception("Category count mismatch!");
        }
    }

    void PrepareCategoryRoles(
        IEnumerable<PhotoCategoryRole> photoCategoryRoles,
        IEnumerable<VideoCategoryRole> videoCategoryRoles
    ) {
        foreach(var cr in photoCategoryRoles)
        {
            var targetCategoryRole = new Models.Target.CategoryRole
            {
                CategoryId = _photoCategoryIdMap[cr.CategoryId],
                RoleId = _roleIdMap[cr.RoleId]
            };

            _target.CategoryRoles.Add(targetCategoryRole);
        }

        foreach(var cr in videoCategoryRoles)
        {
            var targetCategoryRole = new Models.Target.CategoryRole
            {
                CategoryId = _videoCategoryIdMap[cr.CategoryId],
                RoleId = _roleIdMap[cr.RoleId]
            };

            _target.CategoryRoles.Add(targetCategoryRole);
        }
    }

    void PrepareMedia(
        IEnumerable<Photo> photos,
        IEnumerable<Video> videos
    ) {
        // set media chronologically based on category order
        foreach(var category in _target.Categories)
        {
            if(category.LegacyType == 'p')
            {
                _target.Media.AddRange(
                    photos
                        .Where(x => x.CategoryId == category.LegacyId)
                        .OrderBy(x => x.Id)
                        .Select(x => {
                            var media = x.ToTarget(
                                _photoCategoryIdMap[category.LegacyId],
                                Guid.Empty,  // TODO
                                Guid.Empty   // TODO
                            );

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
                        .Select(x => {
                            var media = x.ToTarget(
                                _videoCategoryIdMap[category.LegacyId],
                                Guid.Empty,  // TODO
                                Guid.Empty   // TODO
                            );

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
    ) {
        foreach(var comment in photoComments)
        {
            var targetComment = new Models.Target.Comment
            {
                Id = Guid.CreateVersion7(),
                MediaId = _photoIdMap[comment.PhotoId].Id,
                Created = comment.EntryDate,
                CreatedBy = _userIdMap[comment.UserId],
                Modified = DateTime.MinValue,
                Body = comment.Message
            };

            _target.Comments.Add(targetComment);
        }

        foreach(var comment in videoComments)
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

    void PrepareRatings(
        IEnumerable<PhotoRating> photoRatings,
        IEnumerable<VideoRating> videoRatings
    ) {
        foreach(var rating in photoRatings)
        {
            var targetRating = new Models.Target.Rating
            {
                MediaId = _photoIdMap[rating.PhotoId].Id,
                CreatedBy = _userIdMap[rating.UserId],
                Created = DateTime.MinValue,
                Modified = DateTime.MinValue,
                Score = rating.Score
            };

            _target.Ratings.Add(targetRating);
        }

        foreach(var rating in videoRatings)
        {
            var targetRating = new Models.Target.Rating
            {
                MediaId = _videoIdMap[rating.VideoId].Id,
                CreatedBy = _userIdMap[rating.UserId],
                Created = DateTime.MinValue,
                Modified = DateTime.MinValue,
                Score = rating.Score
            };

            _target.Ratings.Add(targetRating);
        }
    }

    void PrepareLocations(
        IEnumerable<PhotoReverseGeocode> photoReverseGeocodes,
        IEnumerable<Photo> photos,
        IEnumerable<PhotoGpsOverride> photoGpsOverrides,
        IEnumerable<VideoReverseGeocode> videoReverseGeocodes,
        IEnumerable<Video> videos,
        IEnumerable<VideoGpsOverride> videoGpsOverrides
    ) {
        foreach(var reverseGeocode in photoReverseGeocodes)
        {
            var targetMedia = _photoIdMap[reverseGeocode.PhotoId];

            if(reverseGeocode.IsOverride) {
                var gpsOverride = photoGpsOverrides
                    .Single(x => x.PhotoId == reverseGeocode.PhotoId);

                var targetLocation = reverseGeocode.ToTarget(
                    gpsOverride.Latitude,
                    gpsOverride.Longitude
                );

                targetMedia.LocationOverrideId = targetLocation.Id;

                _target.Locations.Add(targetLocation);
            } else {
                var photo = photos
                    .Single(x => x.Id == reverseGeocode.PhotoId);

                var targetLocation = reverseGeocode.ToTarget(
                    photo.GpsLatitude,
                    photo.GpsLongitude
                );

                targetMedia.LocationId = targetLocation.Id;

                _target.Locations.Add(targetLocation);
            }
        }

        foreach(var reverseGeocode in videoReverseGeocodes)
        {
            var targetMedia = _videoIdMap[reverseGeocode.VideoId];

            if(reverseGeocode.IsOverride) {
                var gpsOverride = videoGpsOverrides
                    .Single(x => x.VideoId == reverseGeocode.VideoId);

                var targetLocation = reverseGeocode.ToTarget(
                    gpsOverride.Latitude,
                    gpsOverride.Longitude
                );

                targetMedia.LocationOverrideId = targetLocation.Id;

                _target.Locations.Add(targetLocation);
            } else {
                var photo = videos
                    .Single(x => x.Id == reverseGeocode.VideoId);

                var targetLocation = reverseGeocode.ToTarget(
                    photo.GpsLatitude,
                    photo.GpsLongitude
                );

                targetMedia.LocationId = targetLocation.Id;

                _target.Locations.Add(targetLocation);
            }
        }

        // TODO: look for duplicates based on gps coords
    }

    void PreparePointsOfInterest(
        IEnumerable<PhotoPointOfInterest> photoPointsOfInterest,
        IEnumerable<VideoPointOfInterest> videoPointsOfInterest
    ) {
        foreach(var poi in photoPointsOfInterest)
        {
            var targetMedia = _photoIdMap[poi.PhotoId];

            if(poi.IsOverride)
            {
                _target.PointsOfInterest.Add(new Models.Target.PointOfInterest
                {
                    LocationId = targetMedia.LocationOverrideId,
                    Type = poi.PoiType,
                    Name = poi.PoiName
                });
            } else {
                _target.PointsOfInterest.Add(new Models.Target.PointOfInterest
                {
                    LocationId = targetMedia.LocationId,
                    Type = poi.PoiType,
                    Name = poi.PoiName
                });
            }
        }

        foreach(var poi in videoPointsOfInterest)
        {
            var targetMedia = _photoIdMap[poi.VideoId];

            if(poi.IsOverride)
            {
                _target.PointsOfInterest.Add(new Models.Target.PointOfInterest
                {
                    LocationId = targetMedia.LocationOverrideId,
                    Type = poi.PoiType,
                    Name = poi.PoiName
                });
            } else {
                _target.PointsOfInterest.Add(new Models.Target.PointOfInterest
                {
                    LocationId = targetMedia.LocationId,
                    Type = poi.PoiType,
                    Name = poi.PoiName
                });
            }
        }
    }
}
