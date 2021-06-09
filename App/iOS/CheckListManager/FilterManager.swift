//
//  FilterManager.swift
//  Checklyst
//
//  Created by Matematici Developer on 27/05/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation

class FilterManager {
    
    private static let filtersIdentifier = "userFilters"
    
    private lazy var _filters: [FilterEntry] = {
        return loadFilters()
    }()
    
    private static var instance: FilterManager?
    
    public static var shared: FilterManager {
        if instance == nil {
            instance = FilterManager()
        }
        return instance!
    }
    
    var filters: [FilterEntry] {
        get {
            return _filters
        }
    }
    
    subscript(name: String) -> String? {
        return filters.first(where: { $0.name == name})?.value
    }
    
    func setFilter(name: String, value: String) {
        if let index = _filters.firstIndex(where: { $0.name == name}) {
            _filters[index].value = value
        } else {
            _filters.append(FilterEntry(name: name, value: value))
        }
        saveFilters()
    }
    
    func clearFilters() {
        _filters = []
        UserDefaults.standard.set(nil, forKey: FilterManager.filtersIdentifier)
    }
    
    private func saveFilters() {
        guard let data = try? JSONEncoder().encode(_filters) else {
            print("Error in saving filters")
            return
        }
        UserDefaults.standard.set(data, forKey: FilterManager.filtersIdentifier)
    }
    
    private func loadFilters() -> [FilterEntry] {
        guard let json = UserDefaults.standard.data(forKey: FilterManager.filtersIdentifier),
              let filters = try? JSONDecoder().decode([FilterEntry].self, from: json) else {
            return []
        }
        
        return filters
    }
}

struct FilterEntry: Codable {
    
    public var name: String
    public var value: String
    
    init(name: String, value: String) {
        self.name = name
        self.value = value
    }

}
