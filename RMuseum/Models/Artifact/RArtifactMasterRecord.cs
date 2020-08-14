﻿using System;
using System.Collections.Generic;

namespace RMuseum.Models.Artifact
{
    /// <summary>
    /// Museum Master Item
    /// </summary>
    public class RArtifactMasterRecord
    {

        public RArtifactMasterRecord() { }
        public RArtifactMasterRecord(string name, string description)
        {
            Name = NameInEnglish = name;
            Description = DescriptionInEnglish = description;
        }

        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Friendly Url
        /// </summary>
        public string FriendlyUrl { get; set; }

        /// <summary>
        /// Publish Status
        /// </summary>
        public PublishStatus Status { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    
        /// <summary>
        /// Name In English
        /// </summary>
        public string NameInEnglish { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Description In English
        /// </summary>
        public string DescriptionInEnglish { get; set; }      
        

        /// <summary>
        /// Date/Time
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Last Modified for caching purposes
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Cover Item Index
        /// </summary>
        public int CoverItemIndex { get; set; }

        /// <summary>
        /// Cover Image
        /// </summary>
        public RPictureFile CoverImage { get; set; }

        /// <summary>
        /// Cover Image Id
        /// </summary>
        public Guid CoverImageId { get; set; }

        /// <summary>
        /// Parts of this item
        /// </summary>
        public ICollection<RArtifactItemRecord> Items { get; set; }

        /// <summary>
        /// Item Count (for lists and queries)
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Attributes
        /// </summary>
        public ICollection<RTagValue> Tags { get; set; }
    }
}
